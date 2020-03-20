
// Aivika for .NET
// Copyright (c) 2009-2015  David Sorokin. All rights reserved.
//
// This file is a part of Aivika for .NET
//
// Commercial License Usage
// Licensees holding valid commercial Aivika licenses may use this file in
// accordance with the commercial license agreement provided with the
// Software or, alternatively, in accordance with the terms contained in
// a written agreement between you and David Sorokin, Yoshkar-Ola, Russia. 
// For the further information contact <mailto:david.sorokin@gmail.com>.
//
// GNU General Public License Usage
// Alternatively, this file may be used under the terms of the GNU
// General Public License version 3 or later as published by the Free
// Software Foundation and appearing in the file LICENSE.GPLv3 included in
// the packaging of this file.  Please review the following information to
// ensure the GNU General Public License version 3 requirements will be
// met: http://www.gnu.org/licenses/gpl-3.0.html.

namespace Simulation.Aivika.Experiments.Web

open System
open System.IO
open HtmlTags
open System.Globalization

open Simulation.Aivika
open Simulation.Aivika.Results
open Simulation.Aivika.Experiments

type LastValueStatsProvider () as provider =

    let mutable title = "Last Value Statistics"
    let mutable description = "This section displays the statistics summary collected in final time points."
    let mutable width = 400
    let mutable transform: ResultTransform = id
    let mutable series: ResultTransform = id 
    let mutable filter = eventive { return true }

    member x.Title with get () = title and set v = title <- v
    member x.Description with get () = description and set v = description <- v
    member x.Width with get () = width and set v = width <- v
    member x.Transform with get () = transform and set v = transform <- v
    member x.Series with get () = series and set v = series <- v
    member x.Filter with get () = filter and set v = filter <- v

    interface IExperimentProvider<HtmlDocument> with
        member x.CreateRenderer (ctx) =

            let exp = ctx.Experiment 
            let writer = ctx.Writer
            let formatInfo = exp.FormatInfo
            let runCount = exp.RunCount

            let dict =
                [ for i = 1 to runCount do
                    yield (i - 1, ref []) ] |> Map.ofList

            let names = ref [| |]
            let stats = ref [| |]

            let lockobj = obj ()

            { new IExperimentRenderer with
                member x.BeginRendering () = ()
                member x.Simulate (signals, results) = 
                    eventive {

                        let results = results |> provider.Transform 
                        let results = results |> provider.Series

                        let values = results |> ResultSet.floatStatsChoiceValues |> Seq.toArray

                        let names' = values |> Array.map (fun v -> v.Name)
                        let data'  = values |> Array.map (fun v -> v.Data)

                        lock lockobj (fun () ->
                            if (!names).Length = 0 then
                                names := names'
                                stats := names' |> Array.map (fun v -> SamplingStats.emptyFloats)
                            elif (!names).Length <> names'.Length then
                                failwithf "Series of different lengths are returned for different runs when collecting statistics.")

                        let handle a =
                            eventive {
                                for i = 0 to data'.Length - 1 do
                                    let! st = data'.[i]
                                    lock lockobj (fun () ->
                                        (!stats).[i] <-
                                            (!stats).[i]
                                                |> SamplingStats.appendChoice st)
                            }

                        return! 
                            signals.SignalInStopTime
                                |> Signal.filterc (fun a -> provider.Filter)
                                |> Signal.subscribe handle 
                    }
                member x.EndRendering () =

                    writer.Add("h3")
                          .Text provider.Title |> ignore

                    if provider.Description <> "" then

                        writer.Add("p")
                              .Text provider.Description |> ignore

                    let getDescription (id: ResultId) =
                        match formatInfo.ResultFormatInfo.GetDescription (id) with
                        | Some text -> text
                        | None -> ""
                    
                    let getValue (stats:SamplingStats<float>) formatInfo (r: ResultId) =
                        let value = match r with
                                    | SamplingStatsMeanId -> Convert.ToString (SamplingStats.mean stats, formatInfo)
                                    | SamplingStatsDeviationId -> Convert.ToString (SamplingStats.deviation stats, formatInfo)
                                    | SamplingStatsMinimumId -> Convert.ToString (SamplingStats.minimum stats, formatInfo)
                                    | SamplingStatsMaximumId -> Convert.ToString (SamplingStats.maximum stats, formatInfo)
                                    | SamplingStatsCountId -> Convert.ToString (SamplingStats.count stats, formatInfo)
                                    | _ -> "Unknown value"

                        getDescription r, value

                    let addValue (table: TableTag) (f:ResultDescription, v:string) =
                        let row = table.AddBodyRow()
                        row.Cell().Text(f) |> ignore
                        row.Cell().Text(v) |> ignore

                    for i = 0 to (!names).Length - 1 do

                        let name  = (!names).[i]
                        let stats = (!stats).[i] 

                        let p = writer.Add("p")

                        let table = TableTag()
                        p.Append(table) |> ignore

                        table.Attr("frame", "border")
                             .Attr("cellspacing", string 4)
                             .Attr("width", string provider.Width) |> ignore

                        table.AddBodyRow()
                            .Cell().Attr("colspan", "2")
                            .Add("p").Attr("align", "center")
                            .Text(name) |> ignore

                        let addV = addValue table
                        let v = getValue stats formatInfo

                        addV (v SamplingStatsMeanId)
                        addV (v SamplingStatsDeviationId)
                        addV (v SamplingStatsMinimumId)
                        addV (v SamplingStatsMaximumId)
                        addV (v SamplingStatsCountId)
            }
