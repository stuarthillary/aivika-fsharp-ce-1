namespace Simulation.Aivika.Output

type HtmlOutput = 
    {
        BaseDirectory: string
    }

type TextOutput = 
    {
        BaseDirectory: string
    }

type DatabaseInfo = 
    {
        Info: string
    }

type OutputMethod = 
    | Html of HtmlOutput
    | Text of TextOutput
    | Console
    | Database of DatabaseInfo