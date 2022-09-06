The Cat is #1!!'s submission is a combination of C# with some Python for scripting support.


High level summary of out best strategy:
* If starting blocks: Merge all the blocks into a single block
* Regardless of initial state: Paint the canvas solid white
* Paint rectangles that all have one corner in common at one of the corners of the canvas and overlap them so that you can get fine detail without having to have small blocks.
* Painting a rectangle involves a single point cut, painting one of those blocks, and three merges to get the canvas back to a single block.
* The color of a rectangle was chosen knowing what parts would show in the final painting. We found that median color gave a better result than average color.
* For some problems we manually placed rectangles using our visualizer, other we randomly placed them.
* We used hill climbing to improve the rectangle placements and to remove rectangles.

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
* RoboLars: Attempts to place rectangles automatically for HillClimber.
* BetterLars: RoboLars, but Better!
* RoboRotator:


How Solving Actually Worked:
* Human-assisted solving started with selecting a small number of areas of interest in the visualizer for an AI to use, under the belief that a human would be much better at finding these than an AI. This initial approach wasn't especially effective. After an epiphany on the chessboard problem, the technique evolved into selecting hundreds of overlapping rectangles all anchored in the same corner, highest-priority first, slowly expanding outward. The non-overlapping portion of a newly placed rectangle would cover the largest similar-color area possible, generally only in the rectangle's corner. Because the cost of rectangles decrease the larger the rectangle is, more rectangles would be spent on details further from the starting corner.

Thanks for the contest. We were worried about the whole organizational issues
thing before hand, but this was a really solid contest, plus or minus some
implementation mid contest.
