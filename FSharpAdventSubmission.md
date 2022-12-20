# FSharp Advent Submission 2022: Bayesian Optimization for Performance Tuning in FSharp

## Introduction

For my 6th F# Advent submission (6 years in a row!!), I worked on combining what I learnt from my last 2 submissions: [2020: Bayesian Inference in FSharp](http://bit.ly/3hhhRjq) and [2021: Perf Avore: A Performance Analysis and Monitoring Tool in FSharp](https://github.com/MokoSan/PerfAvore/blob/main/AdventSubmission.md#perf-avore-a-performance-analysis-and-monitoring-tool-in-fsharp) to develop a Bayesian Optimization based algorithm in F# that's compatible with ETW tracing in an effort to discern the best parameters to use based on the specified workload.

The **goal** of this article is to highlight the develop Bayesian optimization algorithm in FSharp that'll help us choose the most optimum parameters based on an objective function for a particular workload. Before jumping in into the specifics of the previous seemingly convoluted sentence, I want to highlight that the goal of any mathematical optimization function is to maximize or minimize a given objective function based on choosing the 

As always, this has been an awesome experience developing with F#.

#### Example

#### Bayesian Optimization

##### Why Use Bayesian Optimization?

### Gaussian Processes

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

## Conclusion

## References

1. [Gaussian Processes](http://krasserm.github.io/2018/03/19/gaussian-processes/)
2. [More About Gaussian Processes](https://peterroelants.github.io/posts/gaussian-process-tutorial/)
3. [Bayesian Optimization From Scratch](https://machinelearningmastery.com/what-is-bayesian-optimization/)
4. [Gaussian Processes for Dummies](http://katbailey.github.io/post/gaussian-processes-for-dummies/)
5. [Heavily Inspired by this Repository](https://github.com/koryakinp/GP)
6. [More About the Squared Exponential Kernel](https://peterroelants.github.io/posts/gaussian-process-kernels/#Exponentiated-quadratic-kernel)
