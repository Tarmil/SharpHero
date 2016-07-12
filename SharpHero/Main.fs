namespace SharpHero

open WebSharper
open WebSharper.Sitelets
open WebSharper.UI.Next
open WebSharper.UI.Next.Server

type EndPoint =
    | [<EndPoint "/">] Home
    | [<EndPoint "/about">] About

module Templating =
    open WebSharper.UI.Next.Html

    type MainTemplate = Templating.Template<"Main.html">

    // Compute a menubar where the menu item for the given endpoint is active
    let MenuBar (ctx: Context<EndPoint>) endpoint : Doc list =
        let ( => ) txt act =
             liAttr [if endpoint = act then yield attr.``class`` "active"] [
                aAttr [attr.href (ctx.Link act)] [text txt]
             ]
        [
            li ["Home" => EndPoint.Home]
            li ["About" => EndPoint.About]
        ]

    let Main ctx action title body =
        Content.Page(
            MainTemplate.Doc(
                title = title,
                menubar = MenuBar ctx action,
                body = body
            )
        )

module Site =
    open WebSharper.UI.Next.Html

    let HomePage ctx =
        Templating.Main ctx EndPoint.Home "Home" [
            h1 [text "Say Hi to the server!"]
            div [client <@ Client.Main() @>]
        ]

    let AboutPage ctx =
        Templating.Main ctx EndPoint.About "About" [
            h1 [text "About"]
            p [text "This is a template WebSharper client-server application."]
        ]

    let Main =
        Application.MultiPage (fun ctx endpoint ->
            match endpoint with
            | EndPoint.Home -> HomePage ctx
            | EndPoint.About -> AboutPage ctx
        )

    open WebSharper.Suave
    open Suave.Web
    open System.Net
    open Suave.Http

    let config = 
        let port = System.Environment.GetEnvironmentVariable("PORT")
        let ip127  = IPAddress.Parse("127.0.0.1")
        let ipZero = IPAddress.Parse("0.0.0.0")
        {defaultConfig with 
            logger = (if port = null then Suave.Logging.Loggers.saneDefaultsFor Suave.Logging.LogLevel.Verbose 
                      else Suave.Logging.Loggers.saneDefaultsFor Suave.Logging.LogLevel.Warn)
            bindings=[ (if port = null then HttpBinding.mk HTTP ip127 (uint16 8080)
                        else HttpBinding.mk HTTP ipZero (uint16 port)) ]}

    do startWebServer config (WebSharperAdapter.ToWebPart Main)
