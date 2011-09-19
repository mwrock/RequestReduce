#RequestReduce
RequestReduce allows any IIS based website to automaticaly sprite background images into a single optimized PNG as well as combine and minify all CSS in the page's &lt;head/&gt; tag with absolutely no coding beyond a few config tweaks. RequestReduce registers itself as a response filter that will fiter any response of content type text/html. The filter looks for all css links in the &lt;head/&gt; tag and replaces them with a single generated url that contains the combined and minified CSS using optimized sprites [where it can] (http://github.com/mwrock/RequestReduce/wiki/Can-I-make-changes-to-my-CSS-classes-to-optimize-RequestReduce-spriting%3F). RequestReduce uses a quantization algorithm (adapted from Xiaolin Wu's fast optimal color quantizer) and [optipng] (http://optipng.sourceforge.net/) to optimize the generated sprite images producing the smallest file size possible without impacting image quality.

RequestReduce performs these optimizations without [sacrificing the server performance of your web site] (http://github.com/mwrock/RequestReduce/wiki/Will-RequestReduce-impact-server-performance%3F). While the process of finding and generating the sprite image and minifying the CSS is naturally an expensive operation, requests will not block on this operation since RequestReduce performs these operations in a background thread and only once until the CSS changes or it is [explicitly asked to flush its reductions] (http://github.com/mwrock/RequestReduce/wiki/How-can-I-get-RequestReduce-to-refresh-changed-CSS-or-images%3F).

RequestReduce excercises common best practices when serving its css and sprited images ensuring that the appropriate [caching headers] (http://github.com/mwrock/RequestReduce/wiki/Will-browsers-cache-the-css-and-image-files-that-RequestReduce-serves%3F) are sent to the browser so that browsers will not need to pull down a new http response until absolutely necessary. Chances are you will see an immediate rise in your yslow and google page speed tests.

RequestReduce provides several [configuration options] (http://github.com/mwrock/RequestReduce/wiki/RequestReduce-Configuration-options) to support [CDN hosting] (http://github.com/mwrock/RequestReduce/wiki/Can-I-have-all-RequestReduce-CSS-and-sprite-resources-pulled-from-a-CDN%3F), [multiple server environments] (http://github.com/mwrock/RequestReduce/wiki/Will-RequestReduce-sync-css-and-sprite-image-files-accross-all-of-the-web-servers-in-my-web-farm%3F) and more.

##Getting started
1. If you have [Nuget] (http://docs.nuget.org/docs/start-here/installing-nuget), simply enter `Install-Package RequestReduce` in the Package Manager Console and skip steps two and three, otherwise download the latest RequestReduce version [here] (http://www.requestreduce.com/)
2. Extract the contents of the downloaded zip and copy RequestReduce.dll and optipng.exe to your website's bin directory
3. Add the RequestReduceModule to your web.config or using the IIS GUI. 

Assuming you are using IIS 7, you would add it by ensuring that your web.config's system.webServer/modules element contains an add element as follows:

    <system.webServer>
        <modules runAllManagedModulesForAllRequests="true">
            <add name="RequestReduce" type="RequestReduce.Module.RequestReduceModule, RequestReduce, Version=1.0.0.0, Culture=neutral" />
        </modules>
    </system.webServer>

**All background images you want to sprite [must have an explicit width in their css class] (http://github.com/mwrock/RequestReduce/wiki/Can-I-make-changes-to-my-CSS-classes-to-optimize-RequestReduce-spriting%3F).** Otherwise RequestReduce cannot guarantee that the background positions it injects will not cause adjacent sprites to bleed into a background image's view port. Also, RequestReduce will ignore repeating images so make sure to mark the image **no-repeat** if it is not a repeating image.

##Requirements
* Thus far, RequestReduce has only been tested using the .Net framework version 4 on IIS 7
* The identity that your asp.net worker process runs under must have write access to your web root directory for creating sprite and css files

##What's Next?
There are a ton of features I intend to add in order to make web performance optimizations just happen as part of installing Request Reduce. Here is what's at the top of the backlog:

* Merge and minify javascript with ability to defer loading
* Leverege CSS 3 to allow RequestReduce to sprite more images without any need for css modifications for supporting browsers
* Options to sprite foreground images
* Provide a command line utility for incorporating RequestReduce optimizations into a build task

##Resources
* Read more about what is available in Request Reduce on the [wiki] (http://github.com/mwrock/RequestReduce/wiki).
* Follow [@mwrockx] (http://twitter.com/mwrockx) for updates on twitter
* Report a bug or suggest a feature [here] (http://github.com/mwrock/RequestReduce/issues)

##License
Licenced under [Apache 2.0] (http://www.apache.org/licenses/LICENSE-2.0)