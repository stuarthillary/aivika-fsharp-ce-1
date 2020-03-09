
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
open HtmlTags

open Simulation.Aivika
open Simulation.Aivika.Experiments

type ExperimentSpecsProvider () as provider =

    let mutable title = "Experiment Specs"
    let mutable description = "It shows the experiment specs"
    let mutable width = 400

    member x.Title with get() = title and set v = title <- v
    member x.Description with get () = description and set v = description <- v
    member x.Width with get () = width and set v = width <- v

// Need to create our own abstraction
// Writing experiment results should not rely on a specific method e.g. creating html

    interface IExperimentProvider<HtmlDocument> with
        member x.CreateRenderer (ctx) =
            { new IExperimentRenderer with
                member x.BeginRendering () = ()
                member x.Simulate (signals, results) = eventive {
                        return { new IDisposable with
                            member x.Dispose () = () } }
                member x.EndRendering () =

                    let exp = ctx.Experiment 
                    let specs = exp.Specs
                    let doc = ctx.Writer
                    let formatInfo = exp.FormatInfo 

                    let body = doc.Body

                    body.Add("h3").Text(doc.Title) |> ignore

                    if provider.Description <> "" then
                        body.Add("p").Text(provider.Description) |> ignore

                    let p = body.Add("p")

                    let table = TableTag()
                    p.Append(table) |> ignore

                    table.Attr("frame", "border")
                         .Attr("cellspacing", string 4)
                         .Attr("width",  string provider.Width) |> ignore

                    table.AddBodyRow()
                         .Cell().Attr("colspan", "2")
                         .Add("p").Attr("align", "center")
                         .Text(provider.Title) |> ignore

                    let r = table.AddBodyRow()
                    r.Cell(formatInfo.StartTimeText) |> ignore
                    r.Cell(Convert.ToString (specs.StartTime, formatInfo)) |> ignore

                    let r = table.AddBodyRow()
                    r.Cell(formatInfo.StopTimeText) |> ignore
                    r.Cell(Convert.ToString (specs.StopTime, formatInfo)) |> ignore

                    let r = table.AddBodyRow()
                    r.Cell(formatInfo.DTText) |> ignore
                    r.Cell(Convert.ToString (specs.DT, formatInfo)) |> ignore

                    let r = table.AddBodyRow()
                    r.Cell(formatInfo.RunCount) |> ignore
                    r.Cell(Convert.ToString (exp.RunCount, formatInfo)) |> ignore

                    let r = table.AddBodyRow()
                    r.Cell(formatInfo.IntegMethod) |> ignore

                    match specs.Method with
                    | Euler -> r.Cell (formatInfo.EulerText) 
                    | RungeKutta2 -> r.Cell (formatInfo.RungeKutta2Text)
                    | RungeKutta4 -> r.Cell (formatInfo.RungeKutta4Text) 
                    |> ignore
             }
