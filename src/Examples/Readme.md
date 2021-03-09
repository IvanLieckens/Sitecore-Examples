﻿# Examples
General repository of examples.

## Contains

### Archive Versions Agent
Agent to automatically archive versions

### Content Editor Timezone
Enable Content Editors to choose their Timezone. All datetime in Sitecore is stored in UTC but by default rendered in Server Time, this allows each individual Content Editor to set their timezone from which all datetime input will be converted for UTC storage.

### Wildcard usage example
1. Shows very simple Standard MVC Site implementation
1. Has an example of using Buckets
1. Has an example of using Content Search
1. Has an example of using Insert Option Rules
1. Has an example of a custom condition rule leveraged by the Insert Option Rule
1. Has an example of a custom bucket naming rule

:warning: Note that this is an example and not production code, many assumptions and shortcuts are present. Feel free to submit a PR or Issue if you want this further improved.

When you implement a Wildcard you have to ask yourself the following questions:
- Do I want centralized or per item based presentation?
  - This example uses centralized
  - Even when using centralized presentation you can still make exceptions for 1 or more items by introducing an item with the exact matching name on the same level as the wildcard. This will take precedence in the item resolving.
  - To use item based presentation you can act during Item Resolving to set the context item to the actual item and not the Wildcard itself. This has the advantage that everything drawing from the context will function as expected. This can be important when using profiling for example.
- How will I resolve the URL to a wildcard item?
  - This example keeps things extremely simple and customly handles this specific usage of the components in the controllers. In real projects you might need more complex handling or might be dealing with multi-level wildcards. It also does a simple direct replacement of the wildcard in the generated URL with the chosen identifier.
  - A Sitecore instance OOTB only supports 1 ItemUrlBuilder, it does support multiple LinkProviders but only 1 selected by default for 1 LinkManager. So this needs to be synched across all sites on the platform or SXA or a custom solution must be leveraged to allow for per site LinkProviders.
- How can I identify the data uniquely?
  - This example uses a person's name but this is flawed since multiple people can have the same name so you might need to consider adding additional information into the URL to allow identification while retaining a human readable URL which fits with your SEO efforts.
- Does the data support live access?
  - This example uses an item bucket and the Content Search API to draw data which will be scaled along with the Content Delivery infrastructure but if you rely on external datasources for drawing the data that will be used in the wildcard presentation you will need to consider whether it will be able to cope with the load generated by people visiting.