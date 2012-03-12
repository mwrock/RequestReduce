#RequestReduce
RequestReduce Makes your website faster - sometimes much faster - with almost no effort

* Auto generates sprites from your background images
* Optimizes Sprite PNG format and compression
* Minifies CSS and Javascript
* Optimizes [caching headers and ETags] (http://github.com/mwrock/RequestReduce/wiki/Will-browsers-cache-the-css-and-image-files-that-RequestReduce-serves%3F)
* Runs on any IIS web site including Classic ASP and PHP
* Can sync content accross [multiple web servers] (http://github.com/mwrock/RequestReduce/wiki/Will-RequestReduce-sync-css-and-sprite-image-files-accross-all-of-the-web-servers-in-my-web-farm%3F)
* Works well with [CDNs] (http://github.com/mwrock/RequestReduce/wiki/Can-I-have-all-RequestReduce-CSS-and-sprite-resources-pulled-from-a-CDN%3F)
* Compiles [Less, Sass and Coffee script] (https://github.com/mwrock/RequestReduce/wiki/Less%2C-Sass-and-Coffee-Script-Compilation)

##Getting started
1. If you have [Nuget] (http://docs.nuget.org/docs/start-here/installing-nuget), simply enter `Install-Package RequestReduce` in the Package Manager Console and skip steps two and three, otherwise download the latest RequestReduce version [here] (http://www.requestreduce.com/)
2. Extract the contents of the downloaded zip and copy RequestReduce.dll and optipng.exe to your website's bin directory
3. If you want to sync multiple servers with SqlServer copy te DLLs in RequestReduce.SqlServer to your bin directory
4. If you have Less, Sass or Coffee scripts, copy the DLLs from RequestReduce.SassLessCoffee to your bin to compile these to CSS and Javascript
5. Add the RequestReduceModule to your web.config or using the IIS GUI. 

Assuming you are using IIS 7, you would add it by ensuring that your web.config's system.webServer/modules element contains an add element as follows:

    <system.web>
      <httpModules>
        <add name="RequestReduce" type="RequestReduce.Module.RequestReduceModule, RequestReduce" />
      </httpModules>
    </system.web>
    <system.webServer>
      <validation validateIntegratedModeConfiguration="false"/>  
		<modules>
          <add name="RequestReduce" type="RequestReduce.Module.RequestReduceModule, RequestReduce" />
		</modules>
	</system.webServer>

**All background images you want to sprite [must have an explicit width in their css class] (http://github.com/mwrock/RequestReduce/wiki/Can-I-make-changes-to-my-CSS-classes-to-optimize-RequestReduce-spriting%3F).** Otherwise RequestReduce cannot guarantee that the background positions it injects will not cause adjacent sprites to bleed into a background image's view port. Also, RequestReduce will ignore repeating images so make sure to mark the image **no-repeat** if it is not a repeating image.

##Troubleshooting
If RequestReduce does not appear to be doing anything, check out this [troubleshooting wiki] (https://github.com/mwrock/RequestReduce/wiki/RequestReduce-is-not-working.-I-don%27t-see-any-spriting-or-minification.-How-can-I-troubleshoot-this%3F) which provides several scenarios, options and debugging tips for figuring out why your content may not be being reduced. Also check the [list of wiki support pages] (https://github.com/mwrock/RequestReduce/wiki) which provides documentation addressing several topics to help you optimize RequestReduce and explain how RequestReduce works.

##Requirements
* The Core RequestReduce Dll is Compatible with Framework versions 3.5 and 4.0
* Sql Server integration and Less, Sass and Coffee compilation require .net 4.0
* Standard Features are Medium Trust compliant
* The identity that your asp.net worker process runs under must have write access to your web root directory for creating sprite and css files

##What's Next?
There are a ton of features I intend to add in order to make web performance optimizations just happen as part of installing Request Reduce. Here is what's at the top of the backlog:

* Support defered javascript loading
* Leverege CSS 3 to allow RequestReduce to sprite more images without any need for css modifications for supporting browsers
* Options to sprite foreground images
* Provide a command line utility for incorporating RequestReduce optimizations into a build task

##Resources
* Read more about what is available in Request Reduce on the [wiki] (http://github.com/mwrock/RequestReduce/wiki).
* Follow [@mwrockx] (http://twitter.com/mwrockx) for updates on twitter
* Report a bug or suggest a feature [here] (http://github.com/mwrock/RequestReduce/issues)
* Get details on upgrades, upcoming features and case studies on my blog at Http://mattwrock.com

##Acknowledgements
RequestReduce uses the following excellent OSS and other Free projects:

###RequestReduce Core
* Microsoft Ajax Minifier licensed under Apache 2.0 : http://ajaxmin.codeplex.com/
* StructureMap by Jeremy D. Miller, The Shade Tree Developer and Joshua Flanagan licensed under Apache 2.0 : http://structuremap.net/structuremap/
* nQuant by Matt Wrock licensed under Apache 2.0 : http://nquant.codeplex.com

###RequestReduce.SqlServer
* Peta Poco by TopTen Software licensed under Apache 2.0 : http://www.toptensoftware.com/petapoco/

###RequestReduce.SassLessCoffee
* .less by Christopher Owen, Erik van Brakel, Daniel Hoelbling and James Foster licensed under Apache 2.0 : http://www.dotlesscss.org/
* SassAndCoffee by Paul Betts under the Microsoft Public License (Ms-PL) : https://github.com/xpaulbettsx/SassAndCoffee

###RequestReduce Internal Code (testing and building)
* XUnit by Brad Wilson and Jim Newkirk under the Microsoft Public License (Ms-PL) : http://xunit.codeplex.com/
* Moq by Clarius, Manas and InSTEDD under BSD License: http://code.google.com/p/moq/
* PSake by James Kovacs under MIT License : http://code.google.com/p/psake/
* ILMerge by Mike Barnett licensed under Apache 2.0 : http://research.microsoft.com/en-us/people/mbarnett/ILMerge.aspx
* TestDriven.net by Mutant Design : http://www.testdriven.net
* Resharper by Jetbrains under OS License : http://www.jetbrains.com/resharper/

##License
Licenced under [Apache 2.0] (http://www.apache.org/licenses/LICENSE-2.0)