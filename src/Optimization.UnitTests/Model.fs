﻿module Model

open System

open NUnit.Framework

open MathNet.Numerics

open Optimization.Model
open Optimization.Domain

[<Test>]
let ``Maximize Sin Between -π and π`` () =
    let gaussianProcess : GaussianProcess = 
        createProcessWithSquaredExponentialKernel { LengthScale = 1.; Variance = 1. }

    let sinObjectiveFunction : ObjectiveFunction = QueryContinuousFunction Trig.Sin
    let model                : GaussianModel     = createModel gaussianProcess sinObjectiveFunction ExpectedImprovement -Math.PI Math.PI 20_000 
    let optima               : OptimaResult      = findOptima { Model = model; Goal = Goal.Max; Iterations = 10 }
    Assert.AreEqual(optima.Optima.X, Math.PI / 2., 0.001)

[<Test>]
let ``Minimize $$ x^2 - x $$ between 50 and 100`` () =
    let gaussianProcess : GaussianProcess = 
            createProcessWithSquaredExponentialKernel  { LengthScale = 0.1; Variance = 1. }

    let sinObjectiveFunction : ObjectiveFunction = QueryContinuousFunction (fun input -> (input * input) - input)
    let model                : GaussianModel     = createModel gaussianProcess sinObjectiveFunction ExpectedImprovement 50 100 300 
    let optima               : OptimaResult      = findOptima { Model = model; Goal = Goal.Min; Iterations = 30 }
    Assert.AreEqual(optima.Optima.X, 50, 0.001)

[<Test>]
let ``Maximize $$ x^2 - x $$ between 50 and 100`` () =
    let gaussianProcess : GaussianProcess = 
            createProcessWithSquaredExponentialKernel  { LengthScale = 0.1; Variance = 1. }

    let sinObjectiveFunction : ObjectiveFunction = QueryContinuousFunction (fun input -> (input * input) - input)
    let model                : GaussianModel     = createModel gaussianProcess sinObjectiveFunction ExpectedImprovement 50 100 300 
    let optima               : OptimaResult      = findOptima { Model = model; Goal = Goal.Max; Iterations = 30 }
    Assert.AreEqual(optima.Optima.X, 100, 0.01)

[<Test>]
let ``Minimize $$ x^2 - 98x + 4 $$ between 0 and 100`` () =
    let gaussianProcess : GaussianProcess = 
        createProcessWithSquaredExponentialKernel { LengthScale = 0.1; Variance = 1. }

    let sinObjectiveFunction : ObjectiveFunction = QueryContinuousFunction (fun input -> (input * input) - (98. * input) + 4.) 
    let model                : GaussianModel     = createModel gaussianProcess sinObjectiveFunction ExpectedImprovement 0 100 300 
    let optima               : OptimaResult      = findOptima { Model = model; Goal = Goal.Min; Iterations = 30 }
    Assert.AreEqual(optima.Optima.X, 4, 0.01)

[<Test>]
let ``Maximize $$ x^2 - 98x + 4 $$ between 0 and 100`` () =
    let gaussianProcess : GaussianProcess = 
        createProcessWithSquaredExponentialKernel { LengthScale = 0.1; Variance = 1. }

    let sinObjectiveFunction : ObjectiveFunction = QueryContinuousFunction (fun input -> (input * input) - (98. * input) + 4.) 
    let model                : GaussianModel     = createModel gaussianProcess sinObjectiveFunction ExpectedImprovement 0 100 300 
    let optima               : OptimaResult      = findOptima { Model = model; Goal = Goal.Max; Iterations = 30 }
    Assert.AreEqual(optima.Optima.X, 100, 0.01)
