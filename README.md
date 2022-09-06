Note: At the time of this writting we have not yet made the repo public but we likely will so check https://github.com/thecatisnumber1/icfp2022/ for an updated version.

# The Cat Is #1!! ICFP 2022 Writeup

The Cat is #1!!'s submission is a combination of C# with some Python for scripting support.

## High level summary of out best strategy:
* If starting blocks: Merge all the blocks into a single block
* Regardless of initial state: Paint the canvas solid white
* Paint rectangles that all have one corner in common at one of the corners of the canvas and overlap them so that you can get fine detail without having to have small blocks.
* Painting a rectangle involves a single point cut, painting one of those blocks, and three merges to get the canvas back to a single block.
* The color of a rectangle was chosen knowing what parts would show in the final painting. We found that median color gave a better result than average color.
* For some problems we manually placed rectangles using our visualizer, other we randomly placed them.
* We used hill climbing to improve the rectangle placements and to remove rectangles.

## Python scripts:
* enrich.py: Updates the blocky problems to have new solutions when we solve earlier problem. Like, when we re-solve 5, it submits a merged 26 and a 40.
* report.py: Creates a report that points out higher value problems by comparing our current solutions to the known best ones.
* bestof.py: Optimizes some solutions to remove dumb things. Was created due to not reading the spec carefully. Really nice of y'all to take the best solutions instead of the most recent.

## C# projects:
* AI: Hosts the various AIs that will be documented below.
* Core: Shared code between the various AIs. Handles the canvas and other tasks.
* Modrian: Command line interface to run AIs on problems.
* Tests: We have tests!?!
* Visualizer: WPF UI frontend for our solvers. Also allows for human guided solutions.

If you're going to run one program, I'd run Visualizer. Building should just
be grabbing VS2022, opening the SLN and pressing compile. No idea if it works
under mono, but probably doesn't.

## Visualizer Instructions
* Select a problem and solver from the dropdowns at the top right. HillClimber and BetterLars are the recommended solvers to run.
* BetterLars should just work when you press Run
* HillClimber requires you to place rectangles first
  * All the mouse interactions here take place on the right side containing the target image
  * First choose the corner from which all rectangles will be drawn by ctrl left clicking the desired corner in the region outside the image but inside of the black border
  * All clicks from now on will place a rectangle from that point to the point you click. You can exit this mode with ctrl-p
  * Every new rectangle you place will be drawn underneath the existing ones.
  * Click run.

## Solvers:
* CheckerBoard
* Scanner
* AllCuts: Naively cuts down to the smallest chunks and fills things in. Mostly for stress-testing the visualizer.
* LinePrinter[HV]: Emits colored lines using the average color of the line.
* DotMatrix: Emits broken up colored lines. Perfect images, terrible scores.
* Swapmaster: For problems with multiple starting blocks. Swaps blocks to their best location.
* HillClimber: Uses manually placed rectangles as a starting spot for hill climbing.
* RoboLars: Uses randomly placed rectangles and hill climbs
* BetterLars: Uses gradient in the target imgage to weight the choice of points
* RoboRotator: Runs RoboLars trying each of the 4 corners as anchors.
* RoboRotator2: Same but uses BetterLars


## Our exploration into this problem:
Our intial ideas mostly involved just splitting the canvas down and painting regions without any merges or swaps. This was terrible because the cost of painting small areas is enormous.

From there we moved to a paradigm of rectangle painting where we would do 2 point cuts, paint 1 block, merge all the blocks back together, and repeat for the next rectangle. One of the advantages of this approach is that rectangles can be painted on top of other rectangles and then colored according to what part doesn't get painted over later.

The optimal color choosing turned out to be difficult and for a long time we just used average color of the target image in that region which was quick to compute using a summed area table. A lot of work went into trying to efficiently compute the average of just the unoccluded parts of each rectangle until we finally just went with a slow but working method of doing this.

Human-assisted solving started with selecting a small number of rectangles in the visualizer for an AI to use, under the belief that a human would be much better at finding these than an AI. The program would use these rectangles as the initial state and improve it with hill climbing. All four edges were allowed to move and the amount by which they can move decreased until eventually no improvements could be found. It then scanned for rectangles that could be removed to improve the score and if any were removed the hillclimbing was restarted. This initial approach wasn't especially effective.

After an epiphany on the chessboard problem, the technique evolved into selecting hundreds of overlapping rectangles all anchored in the same corner chosen in the right order so that, wroughly speaking, the smaller rectangles can paint over the larger ones leaving only a small part of each rectangle visible. The rectangles that were to be painted last are the ones chosen first. The non-overlapping portion of a newly placed rectangle would cover the largest similar-color area possible, generally only in the rectangle's corner. Because the cost of rectangles decrease the larger the rectangle is, more rectangles would be spent on details further from the starting corner.

This was a massive improvement but for some problems we couldn't to figure out where to place the starting rectangles. Even worse it could take 6 hours to run on a single problem. To make this workable we turned the practice of placing all rectangles up against a corner into a hard constraint. Everything turned our to be easier in this world. We were finally able to compute occlusion efficiently, scoring could be done quicker, hill climbing had fewer choices to consider, rectangles went from 2 cuts and 8 merges to 1 cut and 3 merges. It was now practical for us to pick starting states with many random rectangles, hill climb, and get decent results for every problem.

Unfortunately there was one big problem we had punted on that we knew we'd have to fix. We had hardcoded (0, 0) as the corner that all rectangles would be anchored to and for some problems this wasn't the best choice. We had very detailed, hand crafted, rectangle layouts already painstakingly created for problem that required code that didn't make this assumption. No problem, we just transform the problem, optimize it, transform it back. How could it possibly take more than 10 minutes to code? In case I haven't foreshadowed enough, this was the source of many difficult to find bug and much mental anguish. But we eventually got it done and climbed higher still on the leaderboard.

From there we played around with better ways to pick the initial points of our rectangles. We tried biasing toward having more larger rectangles than smaller ones and looking at the gradient of the target image and weighting point selection to favor areas where the color was changing more. We didn't see huge gains from this but eventually we'd see that there were a few cases were the gradient weighting improved things decently.

There was one last major improvement that we would stumble upon. One of our team members announced that he had improved our checkerboard solution by a fair bit more than we thought would be possible with our methods. He had manually taken our solution and replaced all of the off white with white and off black with black. After much consideration we decided to try the median on a per channel basis for our colors. This was much slower but also significantly improved our scores. We had 2 modes the hill climber could use, one where it always used the median throughout the hillclimb and one where it used the average for hill climbing but the median for the final result. Using the median always was definitely better but painfully slow.

We thought about how we could speed up the median code given that it did a sort and then return the middle element, which is O(n log(n)) and we know it can be done in O(n). We decided that the quickest way to get this done would be to find code that someone has already written to do this. So we used some code that someone had posted to Stack Overflow and it crippled our performance and we were too tired to realize this had happened for an embarassingly long amount of time. We eventually reverted it, wrote something simple that worked based on having 256 counts and then sped that up 25% by not running it on the alpha channel.

We ran this on what we could given the remaining time. There were more perf improvements that we knew about but we were too tired to start on another major task by this point so could only eke out a handful of improvements.

Thanks for the contest. We were worried about the whole organizational issues
thing before hand, but this was a really solid contest, plus or minus some
implementation mid contest.
