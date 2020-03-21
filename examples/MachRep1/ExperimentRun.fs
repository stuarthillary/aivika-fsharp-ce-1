
namespace Simulation.Aivika.Examples

open System
open HtmlTags

open Simulation.Aivika
open Simulation.Aivika.Results
open Simulation.Aivika.Experiments
open Simulation.Aivika.Experiments.Web
open Simulation.Aivika.Charting.Web

module Experiment =

    [<EntryPoint>]
    let main args =

        let experiment = Experiment ()

        experiment.Specs <- Model.specs
        experiment.RunCount <- 1000

        let provider1 = ExperimentSpecsProvider ()
        let provider2 = DeviationChartProvider ()
        let provider3 = LastValueStatsProvider ()
        let provider4 = LastValueHistogramProvider ()

        let providers =
            [ provider1 :> IExperimentProvider<HtmlDocument>;
              provider2 :> IExperimentProvider<HtmlDocument>;
              provider3 :> IExperimentProvider<HtmlDocument>;
              provider4 :> IExperimentProvider<HtmlDocument> ]

        experiment.RenderHtml (Model.model, providers)
            |> Async.RunSynchronously

        Console.WriteLine()
        Console.WriteLine("Press Enter...")
        Console.ReadLine () |> ignore

        0
