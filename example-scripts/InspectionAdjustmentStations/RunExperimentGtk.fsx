
#I "../../bin"
#r "../../bin/Simulation.Aivika.dll"
#r "../../bin/Simulation.Aivika.Results.dll"
#r "../../bin/Simulation.Aivika.Experiments.dll"
#r "../../bin/Simulation.Aivika.Charting.Gtk.dll"

#load "Model.fsx"

open System
open System.Web.UI

open Simulation.Aivika
open Simulation.Aivika.Results
open Simulation.Aivika.Experiments
open Simulation.Aivika.Experiments.Web
open Simulation.Aivika.Charting.Gtk.Web

let experiment = Experiment ()

experiment.Specs <- Model.specs
experiment.RunCount <- 1000

let inspectionQueue = ResultSet.findByName "inspectionQueue"
let adjustmentQueue = ResultSet.findByName "adjustmentQueue"

let inspectionStations = ResultSet.findByName "inspectionStations"
let adjustmentStations  = ResultSet.findByName "adjustmentStations"

let outputTimer = ResultSet.findByName "outputArrivalTimer"

let providers =
    [ExperimentProvider.experimentSpecs;
     ExperimentProvider.infiniteQueue inspectionQueue;
     ExperimentProvider.infiniteQueue adjustmentQueue;
     ExperimentProvider.server inspectionStations;
     ExperimentProvider.server adjustmentStations;
     ExperimentProvider.arrivalTimer outputTimer]

experiment.RenderHtml (Model.model, providers)
    |> Async.RunSynchronously
