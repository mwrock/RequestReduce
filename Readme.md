#RequestReduce
RequestReduce allows any IIS based website to automaticaly sprite background images into a single PNG as well as combine and minify all CSS in the page's &lt;head/&gt; tag with absolutely no coding beyond a few config tweaks. RequestReduce registers itself as a response filter that will fiter any responce of content type text/html. The filter looks for all css links in the &lt;head/&gt; tag and replaces them with a single generated url that contains the combined and minified CSS using sprites where it can.

RequestReduce performs these optimizations without sacrificing the performance of your web site. While the process of finding and generating the sprite image and minifying the CSS is naturally an expensive operation, requests will not block on this operation since RequestReduce performs these operations in a background thread and only once until the CSS changes or it is explicitly asked to flush its reductions.

RequestReduce excercises common best practices when serving its css and sprited images ensuring that the appropriate caching headers are sent to the browser ensuring that browsers will not need to pull down a new http response until absolutely necessary. Chances are you will see an immediate rise in your yslow and google page speed tests.

RequestReduce provides several configuration options to support CDN hosting, multiple server environments and more.

##Getting started
1. Download the latest RequestReduce version [here] (https://github.com/mwrock/RequestReduce/downloads)
2. Extract the contents of the downloaded zip and copy RequestReduce.dll to your website's bin directory
3. Add the RequestReduceModule to your web.config or using the IIS GUI. 

Assuming you are using IIS 7, you would add it by ensuring that your web.config's system.webServer/modules element contains an add element as follows:

    <system.webServer>
        <modules runAllManagedModulesForAllRequests="true">
            <add name="RequestReduce" type="RequestReduce.Module.RequestReduceModule, RequestReduce, Version=1.0.0.0, Culture=neutral" />
        </modules>
    </system.webServer>

##Requirements
* Thus far, RequestReduce has only been tested using the .Net framework version 4 on IIS 7
* The identity that your asp.net worker process runs under must have write access to your web root directory for creating sprite and css files
