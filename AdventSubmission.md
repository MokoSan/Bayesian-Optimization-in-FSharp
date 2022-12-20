# FSharp Advent Submission 2022: Bayesian Optimization for Performance Tuning in FSharp

<img src="./resources/Gears.jpg" width="200" height="100">

## Introduction

For my 6th F# Advent submission (6 years in a row!!), I worked on combining what I learnt from my last 2 submissions: [2020: Bayesian Inference in FSharp](http://bit.ly/3hhhRjq) and [2021: Perf Avore: A Performance Analysis and Monitoring Tool in FSharp](https://github.com/MokoSan/PerfAvore/blob/main/AdventSubmission.md#perf-avore-a-performance-analysis-and-monitoring-tool-in-fsharp) to develop a Bayesian Optimization algorithm in F# to solve global optimization problems. This algorithm is compatible with intractable and complex objective functions such as those dictated by the results from Event Tracing for Windows (ETW), a Windows profiling mechanism for troubleshooting and diagnostics, in an effort to discern the best parameters to use based on the specified workload. I plan to demonstrate how I applied the algorithm to various experiments to obtain the best parameters in some practical cases and highlight how I made use of F#'s functional features. In terms of using F#, I have had a fabulous experience ,as always! I have expounded on this [here](#experience-developing-in-fsharp).

If some or all parts of the aforementioned aspects of this submission seem cryptic, fret not as I plan to cover these topics in detail. The intended audience of this submission is any developer, data scientist or performance engineer interested in how a mathematical optimization algorithm is implemented in a functional-first way.

## Goals

To be concrete, the goals of this submission are:

1. [To Describe Bayesian Optimization](#bayesian-optimization) 
2. Go Over the Implementation of the Bayesian Optimization algorithm in F#.
3. Provide a Short Primer on Event Tracing for Windows (ETW).
4. Present the Multiple Applications of the Bayesian Optimization from simple to more complex:
   1. __Optimizing a Mathematical Function__: Finding the maxima of the ``Sin`` function between Pi and π and -π.
   2. __The Wall Clock Time of A Simple Command Line App__: Finding the minima of the wall clock time of execution based on the input. 
   3. __The Percent of Time Spent during Garbage Collection For a High Memory Load Case With Bursty Allocations__: Finding the minima of the percent of time spent during garbage collection based on pivoting on the number of Garbage Collection Heaps or Threads using Traces obtained via ETW.

## Bayesian Optimization

The goal of any mathematical optimization function is the selection of the best element vis-à-vis some criterion known as the objective function from a number of available alternatives; the best element or optima here can be either the one that minimizes the criterion or maximizes it. Mathematically, this can expressed as:

$$ \argmax_x f(x) $$

or finding the argument 'x' that maximizes (minimization is just the negative of maximization) the function, ``f(x)``. 

### Example of Optimization

An example of optimization 


### Why Bayesian Optimization?

Bayesian Optimization is, therefore, meant to find the most optimal element like any other optimization algorithm however, where it shines in comparison to others is when the criterion or objective function is a black box function. A black box function indicates the prospect of a lack of an analytic expression or known mathematical formula for the objective function and/or details about other properties for example, knowing the derivatives or if derivatives exist for the function so as to make use of other optimization techniques such as [Stochastic Gradient Descent](https://en.wikipedia.org/wiki/Stochastic_gradient_descent). 

To summarize, Bayesian Optimization aims to, with the fewest number of steps, find the global optima of a function that could be a black box based function. The efficiency of the Bayesian Optimization algorithm stems from the use of Bayes Theorem to direct the search of the global optima by updating prior beliefs about the state of the objective function in light of new data. For those new to the world of Bayes Theorem, I have written up a summary that can be found [here](https://nbviewer.org/github/MokoSan/FSharpAdvent_2020/blob/main/BayesianInferenceInF%23.ipynb#Bayes-Theorem) as a part of my previous advent submission. 

The two components of the Bayesian Optimization algorithm are the **Surrogate Model** and an **Acquisition Function**. The surrogate model, as hinted to by the name, is a model that serves as an approximation of the objective function while the acquisition function guides where the algorithm should search next where the best observation is most likely to reach the global optima.

#### Surrogate Model

The surrogate model, as mentioned above, is used to approximate the objective function. A number of techniques can be used to represent the surrogate model however, one of the most popular ways to do so is to use __Gaussian Processes__. 

##### Gaussian Processes


#### Acqusition Function


#### Squared Exponential Kernel

## Domain

```fsharp
type Goal =
    | Max
    | Min
```


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

As always, I am **super super** open to feedback as to how I have made use of F# developing this project and so, if any one has suggestions of how I could developed this any better, I am all ears! 

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
