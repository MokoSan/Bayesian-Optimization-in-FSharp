# Bayesian Optimization For Performance Tuning in FSharp

This repository encompasses the work I did for my FSharp Advent 2022 submission that write-up for which can be found [here](AdventSubmission.md); the submission is a good starting point to understand the rationale and underlying concepts.

## Prerequisites

__Experiments__: To get started with viewing the experiments, run [Install.cmd](Install.cmd) that will install the prerequisites to run [PolyGlot Notebooks](https://devblogs.microsoft.com/dotnet/dotnet-interactive-notebooks-is-now-polyglot-notebooks/).

## Basic Usage

```fsharp
// Create a Gaussian Process Model based on the Squared Exponential Kernel for the Sin Function.
let gaussianModelForSin() : GaussianModel =
    let squaredExponentialKernelParameters : SquaredExponentialKernelParameters = { LengthScale = 1.; Variance = 1. }
    let gaussianProcess : GaussianProcess =  createProcessWithSquaredExponentialKernel squaredExponentialKernelParameters
    let sinObjectiveFunction : ObjectiveFunction = QueryContinuousFunction Trig.Sin
    createModel gaussianProcess sinObjectiveFunction -Math.PI Math.PI resolution

// Maximize the Sin Function between -π and π for 10 iterations.
let model         : GaussianModel = gaussianModelForSin()
let optimaResults : OptimaResults = findOptima model Goal.Max 10
printfn "Optima: Sin(x) is maximized when x = %A at %A" optimaResults.Optima.X optimaResults.Optima.Y
```

## Experiments To Highlight Use Cases

## References

1. [Gaussian Processes](http://krasserm.github.io/2018/03/19/gaussian-processes/)
2. [More About Gaussian Processes](https://peterroelants.github.io/posts/gaussian-process-tutorial/)
3. [Bayesian Optimization From Scratch](https://machinelearningmastery.com/what-is-bayesian-optimization/)
4. [Gaussian Processes for Dummies](http://katbailey.github.io/post/gaussian-processes-for-dummies/)
5. [Heavily Inspired by this Repository](https://github.com/koryakinp/GP)
6. [More About the Squared Exponential Kernel](https://peterroelants.github.io/posts/gaussian-process-kernels/#Exponentiated-quadratic-kernel)

## TODO

1. ~~Test out model on a simple Sin function as the objective.~~
2. ~~Create a simple command line app that easily helps us reach some extrema.~~
3. ~~Start working on the infrastructure that orchestrates runs.~~
   1. ~~Criteria:~~
      1. ~~Execution Time~~
      2. ~~Some Aggregate Trace Property~~
4. ~~Adding Discrete Bayesian Optimization.~~
   ~~1. Add the case where we don't try out values we have tried before.~~
5. ~~Abstracting Out The Kernel Method.~~ 
6. ~~Abstracting Out The Acquisition Function.~~
7. ~~Visualization.~~
   1. ~~Save plot as png.~~
   2. ~~Creating Gif Out Of Iterations.~~
8. ~~Creating Notebooks / Experimental Console Apps:~~
   1. ~~Sin Function.~~
   2. ~~Simple Workload.~~
   3. ~~High Memory Load Based Bursty Allocations.~~
9.  Unit Tests.
10. Clean up main logic.
   1. Possibly add computation expressions.
   2. Possibly add ROP-esque behavior.
11. Finish writing the article.
12. Early Stopping.
13. Abstract behavior of the Observation Points.