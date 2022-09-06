The Cat is #1!!'s submission is a combination of C# with some Python for scripting support.

High level strategy:
* If starting blocks: Merge all the blocks into a single block
* Regardless of initial state: Paint the canvas solid white
* Solve the problem as if you added nothing to the spec since the lightning round. 

Python scripts:
* enrich.py: Updates the blocky problems to have new solutions when we solve earlier problem. Like, when we re-solve 5, it submits a merged 26 and a 40.
* report.py: Creates a report that points out higher value problems by comparing our current solutions to the known best ones.
* bestof.py: Optimizes some solutions to remove dumb things. Was created due to not reading the spec carefully. Really nice of y'all to take the best solutions instead of the most recent.

C# projects:
* AI: Hosts the various AIs that will be documented below.
* Core: Shared code between the various AIs. Handles the canvas and other tasks.
* Modrian: Command line interface to run AIs on problems.
* Tests: We have tests!?!
* Visualizer: WPF UI frontend for our solvers. Also allows for human guided solutions.

If you're going to run one program, I'd run Visualizer. Building should just
be grabbing VS2022, opening the SLN and pressing compile. No idea if it works
under mono, but probably doesn't.

AIs:
* CheckerBoard
* Scanner
* AllCuts: Naively cuts down to the smallest chunks and fills things in. Mostly for stress-testing the visualizer.
* LinePrinter[HV]: Emits colored lines using the average color of the line.
* DotMatrix: Emits broken up colored lines. Perfect images, terrible scores.
* Swapmaster: For problems with multiple starting blocks. Swaps blocks to their best location.
* HillClimber:
* RoboLars:
* BetterLars:
* RoboRotator:


How Solving Actually Worked:
* Someone else needs to write this -- pat

Thanks for the contest. We were worried about the whole organizational issues
thing before hand, but this was a really solid contest, plus or minus some
implementation mid contest.
