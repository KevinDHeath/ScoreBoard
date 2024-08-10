## Grass.Play Blazor App

- Switched from using [Bootstrap v5.1.0](https://getbootstrap.com/docs/5.1/getting-started/introduction/) to [Bulma v1.0](https://bulma.io/documentation/start/overview/).
- Moved the sidebar options to the top row.
- Implemented `collapsible sections`.\
 *See* [Apply a render mode to a component definition](https://learn.microsoft.com/en-us/aspnet/core/blazor/components/render-modes?view=aspnetcore-8.0#apply-a-render-mode-to-a-component-definition) for more information on `@rendermode InteractiveServer`.
- Implemented `hide-able` sections.
- Implemented `anchor navigation` for quick links.\
  *See* [Hashed routing to named elements](https://learn.microsoft.com/en-us/aspnet/core/blazor/fundamentals/routing?view=aspnetcore-8.0#hashed-routing-to-named-elements) for more information.
- Implemented `SectionOutlet` for top bar.\
  *See* [ASP.NET Core Blazor sections](https://learn.microsoft.com/en-us/aspnet/core/blazor/components/sections) for more information.
- Implemented `global InteractiveServer` rendering for top bar section updating.\
  *See* [ASP.NET Core Razor component lifecycle](https://learn.microsoft.com/en-us/aspnet/core/blazor/components/lifecycle) for more information.
 