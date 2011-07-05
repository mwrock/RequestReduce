#RequestReduce
RequestReduce allows any IIS based webste to automatically sprite background images into a sinle PNG as well as combine and minify all CSS in the page's &lt;head/&gt; tag with absolutely no coding beyond a few config tweaks. RequestReduce registers itself as a response filter that will fiter any responce of content type text/html. The filter looks for all css links in the &lt;head/&gt; tag and replacesthem with a single generated url that contains the combined and minified cs using sprites where it can.

RequestReduce provides several configuration options to support CDN hosting, multiple server environments and more.

##Getting started
1. Download the latest RequestReduce version [here] (https://github.com/mwrock/RequestReduce/downloads)
2. Extract the contacts ofthe download zip and copy RequestReduce.dll to your bin directory
3. Add the RequestReduceModule to your web.config. Assuming you are using IIS 7, you would add it using this config:

	<system.webServer>
		<modules runAllManagedModulesForAllRequests="true">
			<add name="RequestReduce" type="RequestReduce.Module.RequestReduceModule, RequestReduce, Version=1.0.0.0, Culture=neutral" />
		</modules>
	</system.webServer>

##Requirements
* Thus far, RequestReduce has only been tested using .Net framework version 4 on IIS 7
* The identity that your asp.net worker process runs under must have write access to your web root directory for creating sprite and css files
