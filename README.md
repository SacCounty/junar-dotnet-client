# Junar API .Net Client #
# junar-dotnet-uploader #
Unofficial API Java Client implementation for the Junar.com API, a service to collect, organize, use and share data.
.Net Core based Command Line application, built to be run on Windows and linux.  This utility can be used to identify the unique
 identifier string (referred to as GUID by Junar ) and other properties for both Junar cloud based datasets and data views and allows
  publishing of data views by that GUID.   It requires an auth_key, junar web service URL, GUID, title, etc. as shown below.


## Usage ##

Junar API V1 :  HTTPPOST to publish datastreams 
 junar-dotnet-uploader.exe 
-t "title" 
-d "description" 
-k "userKeyValue" 
-u "http://whoAreYou.cloudapi.junar.com/datastreams/publish" 
-ct "content-Type" 
-c "category" 
-ta "tags" 
-g "guid"
-n "notes"
-f "filePath"

Junar API V2 :  HTTPGET to find datasets 
 junar-dotnet-uploader.exe 
-G true
-k "userKeyValue" 
-u "http://whoAreYou.cloudapi.junar.com/api/v2/datasets.json" 

Junar API V2 :  HTTPGET to find datastreams 
 junar-dotnet-uploader.exe 
-G true
-k "userKeyValue" 
-u "http://whoAreYou.cloudapi.junar.com//api/v2/datastreams.json" 



Junar API for publishing datastreams or data views:
We found that datastreams = data views, and API V2 could separate them out with different ‘GUID’ string identifiers but the original
API would only allow publishing via the datastream GUID.  While API V2 worked great for HTTPGET calls to obtain a list of all pertinent
properties of published datasets as well as data views, we found issues in using this for the publish service.   So, we used the original
API as noted for the HTTPPOST to the junar publish web service.

Required properties for the publish service APIv1 include:
auth_key ( -k )		[ authorization key for accessing Junar resource]
url ( -u )			[ URL endpoint for Junar service]
title	( -t )		[ passed to Junar service and should match existing title ]
description  ( -d )	[ passed to Junar service and should match existing description ]
category ( -c )		[ passed to Junar service and must match existing category ]
tags ( -ta )		[ passed to Junar service for tags ]
guid ( -g )		[ passed to Junar service to identify the data view ]
content-type ( -ct )	[ Used internally to identify the content-type of the file to publish ]
file_path ( -f )		[ Used internally to pick up the file to publish ]


Notes for using Junar publish service directly:

•	For an update to an existing file, you need to include the switch : clone=True
•	You must set the Content-Type in the header to match [.zip = application/octet-stream]
•	Note the multipart request for posting a data file and form data parms and that you must set up a boundary in your Request around the pieces.
•	Debug with fiddler to view your request and the response


## License ##

(Released under MIT License since v0.0.1)

Copyright (c) 2011 Marco Salgado

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the 'Software'), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED 'AS IS', WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
