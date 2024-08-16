![Rock RMS](https://raw.githubusercontent.com/SparkDevNetwork/Rock/develop/Images/github-banner.png)

Rock RMS is an open source Relationship Management System (RMS) and Application
Framework for 501c3 organizations[^1]. While Rock specializes in serving the unique needs of churches it's
useful in a wide range of service industries.  Rock is an ASP.NET 4.5 C# web application
that uses Entity Framework 6.0, jQuery, Bootstrap 3, and many other open source libraries.

Our main developer starting point site is [the wiki](https://github.com/SparkDevNetwork/Rock/wiki).

## Learn More

Jump over to our [Rock website](https://www.rockrms.com/) to find out more. Keep up to date by:

* [Reading our blog](https://community.rockrms.com/connect)
* [Following us on Twitter](https://www.twitter.com/therockrms)
* [Liking us on Facebook](https://www.facebook.com/therockrms)
* [Reading the community Q & A](https://community.rockrms.com/ask)
* [Subscribing to our newsletter](https://www.rockrms.com/Rock/Subscribe)

## License
Rock released under the [Rock Community License](https://www.rockrms.com/license).

## Crafted By

A community of developers led by the [Spark Development Network](https://www.sparkdevnetwork.com/).

## Installer Note

Normally the [Rock installer](https://www.rockrms.com/Download) generates a unique `PasswordKey`
`DataEncryptionKey` and MachineKey's `validationKey` and `decryptionKey`. So if you decide
to clone the repo and run it directly, you will need to handle that aspect yourself.

## Adding new block plugins

A block plugin is made up of two parts: a server part and a client part.

Server part code is added as additional project and client part is added as part of `Rock.JavaScript.Obsidian.Plugins` project.

[Anatomy of an Obsidian block](https://sparkdevnetwork.gitbook.io/obsidian/blocks/creating-blocks#anatomy-of-an-obsidian-block)

#### Server part

Server part blocks are added as part of an additional project so it does not interfere with the Rock core files.

Add new project following the  convention:
`<reversed_domain>.<project_name>`

Once a project is added, a project reference needs to be added to the `Rock Web Site` project so it automatically outputs the DLL file to the web project and it works automatically when running the web project.

Blocks should be added under `/Blocks` inside the project.

By default, Rock expects the client part to be inside the `/Obsidian/Blocks` directory, and since we will output the obsidian client component to the Plugins folder, we need to override the `ObsidianFileUrl` property and because of that all our custom plugin blocks should inherit the custom class that inherits the `RockBlockType` class and overrides the `ObsidianFileUrl` property.

The core files of a block are `Block Attributes`, `GetObsidianBlockInitialization` method and custom actions decorated with `BlockAction` attribute.
[C# Block](https://sparkdevnetwork.gitbook.io/obsidian/blocks/creating-blocks#c-block)

#### Client part

Add new obsidian components and typescript models inside `Rock.JavaScript.Obsidian.Plugins` project under src directory following the directory convention:
`<reversed_domain>/<project_name>/`

`Rock.JavaScript.Obsidian.Plugins` project is configured to run alongside the `Rock Web Site` and it will automatically build and output the src obsidian files to the  `/Plugins` directory.

[Obsidian Component Structure](https://sparkdevnetwork.gitbook.io/obsidian/obsidian-component-structure)\
[TypeScript Component](https://sparkdevnetwork.gitbook.io/obsidian/blocks/creating-blocks#typescript-component)

 [^1]: [See our FAQ for details on our license](https://www.rockrms.com/faq)
