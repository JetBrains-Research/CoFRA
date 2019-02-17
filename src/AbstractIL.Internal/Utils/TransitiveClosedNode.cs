using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cofra.AbstractIL.Internal.Utils
{
    public class TransitiveClosedNode<TData, TOwner>
    {
        private readonly HashSet<TData> myCollected;

        private HashSet<TransitiveClosedNode<TData, TOwner>> myDonors;
        private HashSet<TransitiveClosedNode<TData, TOwner>> myRecipients;

        public ISet<TData> Collected => myCollected;

        public event Action<TData> OnNewDataAdded;

        public IEnumerable<TransitiveClosedNode<TData, TOwner>> Recipients => myRecipients;
        public TOwner Owner { get; set; }

        public TransitiveClosedNode()
        {
            myDonors = new HashSet<TransitiveClosedNode<TData, TOwner>>();
            myRecipients = new HashSet<TransitiveClosedNode<TData, TOwner>>();

            myCollected = new HashSet<TData>();
        }

        public void DropConnections()
        {
            myDonors = null;
            myRecipients = null;
        }

        public bool PutData(TData data)
        {
            if (myCollected.Add(data))
            {
                foreach (var recipient in myRecipients)
                {
                    recipient.myCollected.Add(data);
                }

                OnNewDataAdded?.Invoke(data);

                return true;
            }

            return false;
        }

        public bool Propagate(TransitiveClosedNode<TData, TOwner> target)
        {
            if (!myRecipients.Add(target))
            {
                return false;
            }

            var shouldContinue = true;
            foreach (var data in myCollected.ToList())
            {
                shouldContinue = shouldContinue && target.PutData(data);
            }

            if (!shouldContinue)
            {
                return false;
            }

            var donors = (new[] {this}).Concat(myDonors).ToList();
            var recipients = (new[] {target}).Concat(target.myRecipients);

            foreach (var recipient in recipients)
            {
                foreach (var donor in donors)
                {
                    if (recipient.myDonors.Add(donor))
                    {
                        donor.myRecipients.Add(recipient);
                    }
                }
            }

            return true;
        }
    }
}
