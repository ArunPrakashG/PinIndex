# PinIndex
 
Unoffical .NET standard wrapper library for `api.postalpincode.in` API.
Used to get post offices based on either of these search queries:
	* Pin code (Zip code) of the locality.
	* Branch name of the locality.

## Sample Usage

```
PinIndexIndiaClient piClient = new PinIndexIndiaClient();
Response response = await piClient.RequestByPinCodeAsync(123456,  default).ConfigureAwait(false);
piClient.Dispose();
if(response == null){
	// Error occured.
}

// handle the Response 

```

Or...

```
Response response = default;
using(PinIndexIndiaClient piClient = new PinIndexIndiaClient()){
	response = piClient.RequestByBranchName(BRANCH_NAME);
}

if(response == null){
	// Error occured.
}

// handle the Response 

```

## Dependencies
* Newtonsoft.Json - for parsing api response.

## License
The MIT License (MIT)

Copyright (c) 2020 ArunPrakashG

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NON INFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
