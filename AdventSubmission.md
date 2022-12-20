# FSharp Advent Submission 2022: Bayesian Optimization for Performance Tuning in FSharp

<img src="./resources/Gears.jpg" width="400" height="300">

## Introduction

For my 6th F# Advent submission (6 years in a row!!), I worked on combining what I learnt from my last 2 submissions: [2020: Bayesian Inference in FSharp](http://bit.ly/3hhhRjq) and [2021: Perf Avore: A Performance Analysis and Monitoring Tool in FSharp](https://github.com/MokoSan/PerfAvore/blob/main/AdventSubmission.md#perf-avore-a-performance-analysis-and-monitoring-tool-in-fsharp) to develop a Bayesian Optimization algorithm in F# to solve global optimization problems. This algorithm is compatible with intractable and complex objective functions such as those dictated by the results from Event Tracing for Windows (ETW), a Windows profiling mechanism for troubleshooting and diagnostics, in an effort to discern the best parameters to use based on the specified workload. I plan to demonstrate how I applied the algorithm to various experiments to obtain the best parameters in some practical cases and highlight how I made use of F#'s functional features. In terms of using F#, I have had a fabulous experience ,as always! I have expounded on this [here](#experience-developing-in-fsharp).

If some or all parts of the aforementioned aspects of this submission seem cryptic, fret not as I plan to cover these topics in detail. The intended audience of this submission is any developer, data scientist or performance engineer interested in how the Bayesian Optimization algorithm is implemented in a functional-first way.

## Goals

To be concrete, the goals of this submission are:

1. [To Describe Bayesian Optimization](#bayesian-optimization). 
2. [Present the Multiple Applications of the Bayesian Optimization from simple to more complex](#experiments):
   1. [__Optimizing a Trigonometric Function__: Finding the maxima of the ``Sin`` function between Pi and π and -π.](#experiment-1-objective-function-is-to-maximize-sinx)
   2. __The Wall Clock Time of A Simple Command Line App__: Finding the minima of the wall clock time of execution based on the input. 
   3. __The Percent of Time Spent during Garbage Collection For a High Memory Load Case With Bursty Allocations__: Finding the minima of the percent of time spent during garbage collection based on pivoting on the number of Garbage Collection Heaps or Threads using Traces obtained via Event Tracing For Windows (ETW); I 
3. Provide a Short Primer on Event Tracing for Windows (ETW).

## Bayesian Optimization

In this section, I plan to go over concomitant statistical topics but not dwell too much into the mathematical formula but instead try to give an intuitive meaning as best as possible. The goal of any mathematical optimization function is the selection of the best element vis-à-vis some criterion known as the objective function from a number of available alternatives; the best element or optima here can be either the one that minimizes the criterion or maximizes it. Mathematically, this can expressed as:

$$ \arg \max_{x} f(x) $$

or finding the argument 'x' that maximizes (minimization is just the negative of maximization) the function, ``f(x)``. The "direction" of the optimization i.e. whether we are minimizing or maximizing can be modeled as "Goal" such as:

```fsharp
type Goal =
    | Max
    | Min
```

### Why Bayesian Optimization?

Bayesian Optimization is, therefore, meant to find the most optimal element like any other optimization algorithm however, where it shines in comparison to others is when the criterion or objective function is a black box function. A black box function indicates the prospect of a lack of an analytic expression or known mathematical formula for the objective function and/or details about other properties for example, knowing the derivatives or if derivatives exist for the function so as to make use of other optimization techniques such as [Stochastic Gradient Descent](https://en.wikipedia.org/wiki/Stochastic_gradient_descent). 

To summarize, Bayesian Optimization aims to, with the fewest number of steps, find the global optima of a function that could be a black box based function. 

### What is Bayesian About the Bayesian Optimization Algorithm? 

The efficiency of the Bayesian Optimization algorithm stems from the use of Bayes Theorem to direct the search of the global optima by updating prior beliefs about the state of the objective function in light of new data. For those new to the world of Bayes Theorem, I have written up a summary that can be found [here](https://nbviewer.org/github/MokoSan/FSharpAdvent_2020/blob/main/BayesianInferenceInF%23.ipynb#Bayes-Theorem) as a part of my previous advent submission but here is a quick primer:



## Experiments

### Experiment 1: Optimizing a Trigonometric Function: Finding the maxima of the ``Sin`` function between Pi and π and -π.

``Sin(x)`` is a simple Trigonometric Function whose maximum value is 1 at $\frac{\pi}{2}$. Since we know the analytical form of the function, this simple experiment can highlight the efficacy of the algorithm. A Polyglot notebook of this experiment can be found [here](./Experiments/Sin/Sin.ipynb). 

The experiment can be setup in the following way making use of the library I developed that implements the Bayesian Optimization algorithm:

```fsharp
open Optimization.Domain
open Optimization.Model
open Optimization.Charting

let iterations : int = 100
let resolution : int = 1000 

// Creating the Model.
let gaussianModelForSin() : GaussianModel =
    let gaussianProcess : GaussianProcess = 
        { 
            KernelFunction     = SquaredExponentialKernel { LengthScale = 0.1; Variance = 1 }
            ObservedDataPoints = List<DataPoint>()
            CovarianceMatrix   = Matrix<double>.Build.Dense(1, 1)
        }

    let sinObjectiveFunction : ObjectiveFunction = QueryContinuousFunction Trig.Sin
    createModel gaussianProcess sinObjectiveFunction -Math.PI Math.PI resolution 

// Using the Model to generate the optimaResults.
let model         : GaussianModel = gaussianModelForSin()
let optimaResults : OptimaResults = findOptima model Goal.Max iterations 
printfn "Optima: Sin(x) is maximized when x = %A at %A" optimaResults.Optima.X optimaResults.Optima.Y
```

The result of the experiment was:

``Optima: Sin(x) is maximized when x = 1.575513433 at 0.9999888745``. The optima is very close to $ \frac{\pi}{2} \approx 1.57079632679$ where $sin(x) = 1$ and therefore, we know this algorithm is optimizing, as expected. I have created some charting helpers that chart the pertinent series for each iteration:

```fsharp
[<Literal>]
let BASE_SAVE_PATH = @".\resources\" 

let allCharts : GenericChart.GenericChart seq = chartAllResults optimaResults
saveCharts BASE_SAVE_PATH allCharts |> ignore

let gifPath : string = Path.Combine(BASE_SAVE_PATH, "Combined.gif")
saveGif BASE_SAVE_PATH gifPath |> ignore
```

The output gif that's the combination of the iterations is:

![Sin Function](./Experiments/Sin/resources/Combined.gif)

### Experiment 2: Objective Function is to Minimize ``Sin(x)``

### Experiment 3: Objective Function is to Minimize ``Sin(x)``


### Implementation Of Bayesian Optimization

The two components of the Bayesian Optimization algorithm are the **Surrogate Model** and an **Acquisition Function**. The surrogate model, as hinted to by the name, is a model that serves as an approximation of the objective function while the acquisition function guides where the algorithm should search next where the best observation is most likely to reach the global optima.

#### Surrogate Model

The surrogate model, as mentioned above, is used to approximate the objective function. A number of techniques can be used to represent the surrogate model however, one of the most popular ways to do so is to use __Gaussian Processes__. 

##### Gaussian Processes

A Gaussian Process is a random process comprising of a collection of random variables such that the joint probability distribution of every subset of these random variables is normally distributed. Intuitively, a Gaussian Process can be thought of a possibly infinite set of normally distributed variables that excel at efficient and effective summarization of a large number of functions and smooth transitions as more data is available to the model.

An important aspect of defining a Gaussian Process is the __Kernel__ that controls the shape of the function at specific points. The kernel I have implemented is called the "Squared Exponential Kernel" or "Gaussian Kernel" and has two parameters that control the smoothness of the function via a length parameter and vertical variation via a variance parameter. 

The code of this function looks like the following where ``left`` and ``right`` are the two points for which we get details about the shape of the function at hand.

```fsharp
// src/Optimization.Core/Kernel.fs

let squaredExponentialKernelCompute (parameters : SquaredExponentialKernelParameters) (left : double) (right : double) : double = 
    if left = right then parameters.Variance
    else
        let squareDistance : double = Math.Pow((left - right), 2)
        parameters.Variance * Math.Exp( -squareDistance / ( parameters.LengthScale * parameters.LengthScale * 2. ))
```

where the ``SquaredExponentialKernelParameters`` are defined as:

```fsharp
// src/Optimization.Core/Domain.fs

type SquaredExponentialKernelParameters = { LengthScale : double; Variance : double }
```

More details about the choice of this kernel can be found [here](https://peterroelants.github.io/posts/gaussian-process-kernels/#Exponentiated-quadratic-kernel).

Mathematically,

$$ k(x_a, x_b) = \sigma^2 \exp \left(-\frac{ \left\Vert x_a - x_b \right\Vert^2}{2\ell^2}\right) $$

With:

 - $σ^2$ the overall variance (is also known as amplitude or the vertical variance).
 - $\ell$ the length scale gives us the smoothness between the two points.

#### Acquisition Function

Where to search next is dictated by the acquisition function. This function conducts a trade off between "Exploitation and Exploration". __Exploitation__ means we are sampling or choosing points where the surrogate model is know to produce high objective function result. __Exploration__ means we are sampling or choosing points we haven't explored before or where the prediction uncertainty is high. The point where the acquisition function is maximized is the next point to sample.

A very popular acquisition function is called "Expected Improvement (EI)" and the crux of this function can be represented in F# as:

```fsharp
let Δ : double = predictionResult.Mean - optimumValue - explorationParameter
let σ : double = (predictionResult.UpperBound - predictionResult.LowerBound) / 2.
let z                   : double = Δ / σ 
let exploitationFactor  : double = Δ * Normal.CDF(0, 1, z)
let explorationFactor   : double = σ * Normal.PDF(0, 1, z)
let expectedImprovement : double = exploitationFactor + explorationFactor
```

The idea here is that based on the predicted result from the surrogate function, we compute an exploitation and exploration factor and sum the two. The mean, upper bound and lower bound are all results from the Gaussian Process. 

#### The Bayesian Optimization Loop 

Now that we have gone over the 

#### ETW Events

## Usage of FSharp's Features

1. Domain Modeling
2. Partial Application
3. Type Aliasing
4. Pattern Matching
5. Conditional Mutability
6. Async and Seq Computational Expressions

### PolyGlot Notebooks

## Experience Developing in FSharp

<img src="./resources/IloveFSharp.png" width="500" height="300">

Using F# for this project has been nothing short of **awesome**! After working on these submissions, I am usually left questioning why I don't use F# more often. Even though I didn't use the plethora of features the language has to offer, I lucidly accomplished what I intended to set out to accomplish; I believe the ease with which I was able to get things done is testament to strength of the language that keeps me coming back for more! 

Domain Modeling using Discriminated Unions and Record Types, Pattern Matching and Partial Application are the 3 of my favorite features I had the most fun using. My favorite functional programming concept is one of the core ones: how composing functions with other functions is a simple yet powerful foundation to build complex features on. 

As always, I am **super** open to feedback as to how I have made use of F# developing this project and so, if any one has suggestions of how I could developed this any better, I am all ears! 

## Conclusion

## 6 Years Going Strong!

It has been 6 years of submissions to the FSharp Advent event and it has been an awesome experience. Here are links to my previous posts:

1. [2021: Perf Avore: A Performance Analysis and Monitoring Tool in FSharp](https://github.com/MokoSan/PerfAvore/blob/main/AdventSubmission.md#perf-avore-a-performance-analysis-and-monitoring-tool-in-fsharp)
2. [2020: Bayesian Inference in F#](https://bit.ly/3hhhRjq)
3. [2019: Building A Simple Recommendation System in F#](http://t.co/KqE8kfaZQ7)
4. [2018: An Introduction to Probabilistic Programming in F#](https://t.co/fdssLnvzLX)
5. 2017: The Lord of The Rings: An F# Approach
   1. [Introduction](https://t.co/8qGEiwNniY)
   2. [The Path of the Hobbits](https://t.co/UtFQRj3W3X)
   3. [The Path of the Wizard](https://t.co/6AzIg7voAb)
   4. [The Path of the King](https://t.co/ko6bubJqsw)

## References

1. [Gaussian Processes](http://krasserm.github.io/2018/03/19/gaussian-processes/)
2. [More About Gaussian Processes](https://peterroelants.github.io/posts/gaussian-process-tutorial/)
3. [Bayesian Optimization From Scratch](https://machinelearningmastery.com/what-is-bayesian-optimization/)
4. [Bayesian Optimization](http://krasserm.github.io/2018/03/21/bayesian-optimization/)
5. [Gaussian Processes for Dummies](http://katbailey.github.io/post/gaussian-processes-for-dummies/)
6. [Heavily Inspired by this Repository](https://github.com/koryakinp/GP)
7. [More About the Squared Exponential Kernel](https://peterroelants.github.io/posts/gaussian-process-kernels/#Exponentiated-quadratic-kernel)
