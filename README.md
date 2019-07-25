[![Build status](https://ci.appveyor.com/api/projects/status/4uuha50qe858lcd6/branch/master?svg=true)](https://ci.appveyor.com/project/ilya-nozhkin/cofra/branch/master)
[![Build Status](https://travis-ci.org/gsvgit/CoFRA.svg?branch=master)](https://travis-ci.org/gsvgit/CoFRA)

# Short description.

CoFRA project implements context-free language reachability approach to provide an extensible platform for performing interprocedural static analyses. The core of the project is a service which is responsible for accumulating information about the source code and running different analyses defined in terms of pushdown automata. It provides a socket-based interface for interaction with frontends which are responsible for extracting necessary information and notificating user about issues found by the analysers.

# The plugin

There is also one implementation of a frontend which is based on ReSharper SDK and thus can be installed into ReSharper, Rider and InspectCode. It also contains a bundled backend providing one analysis performing a kind of taint tracking. 
It tracks the data from specially marked fields called sources to methods called sinks checking whether they are passed through filters.
Examples of use can be found in the [tests folder](test/data/TaintAnalysis).

The plugin itself can be downloaded [here](https://ci.appveyor.com/project/ilya-nozhkin/cofra/build/artifacts)
