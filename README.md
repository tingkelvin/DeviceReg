
<!-- PROJECT LOGO -->
<br />
<div align="center">
  </a>
  <h1 align="center">DeviceReg App</h3>
</div>

<!-- ABOUT THE PROJECT -->
## About The Project

![azurefunction](https://grapecitycontentcdn.azureedge.net/blogs/grapecity/20181112-an-introduction-to-azure-functions-/2.png)

The project is about building a RESTful API for customer to register device. This can be done by:
* Building an Azure Http Trigger Function that acts as an API. This API will also consume another API to retrieve data, mainly  the asset id.
* Writing the data to SQL database. The data includes asset id, device id, name, location and type.
* Using infrastructure as code and automated deployment pipeline. The pipeline composed of 5 stages: Create Database, Deploy Database, Build App, Deploy App and Test (Integration)

Performance requirement:
* Retry failure api request
* Process 1000 requests in 10 minutes  

Please see Integration Test Section for quality assessment.

### Built With

This section list any major frameworks/libraries used to bootstrap the project. 
* Dotnet 7.0
* Python 3.X
* Azure Http Trigger Function
* Azure SQL Database
* Azure DevOps

### File Directory
* Sql -> contains script to deploy database
* Test -> contains script to conduct integration test
* model -> contain .net class for data modelling
* utils -> helper functions for azure function
* register.cs -> the main script for azure function
* azure-pipelines.yml -> configuration of azure devops pipeline

### Register Device(s)

The Azure Http Trigger Function end-point:  
https://devicereg.azurewebsites.net/api/register 

To register one single device, make a post request to the endpoint with the following body:

    {
	    "devices": [
		    {
			    "id": "DVID0000001",
			    "Name": "device 1 name",
			    "location": "Melbourne",
			    "type": "Pressure Sensor"
		    }
	    ]
    }

To register multiple devices, make a post request to the endpoint with the following body:

    {
	    "devices": [
		    {
			    "id": "DVID0000001",
			    "Name": "device 1 name",
			    "location": "Melbourne",
			    "type": "Pressure Sensor"
		    },
			   // your other devices
		    {
			    "id": "DVID0000002",
			    "Name": "device 2 name",
			    "location": "Melbourne",
			    "type": "Pressure Sensor"
		    },
	    ]
    }

### Example of using Postman
![postman](https://github.com/tingkelvin/DeviceRegistration/assets/49113121/50c1fb76-570d-49e2-8b43-55fe86c6c063)

### Azure DevOps Pipeline
The azure-pipelines.yml contains the configuration of the pipeline.

![pipeline](https://github.com/tingkelvin/DeviceReg/assets/49113121/dfc1b9de-2f46-45c3-bbd8-64bae38ec252)
There are 5 stages:

 1. Create Database Stage - AzureCLI@1  
    ![createdatabasestage](https://github.com/tingkelvin/DeviceReg/assets/49113121/6b63aac8-8e80-4b8c-ad30-a7bf0bd366c8)
 1. Deploy Database Stage -SqlAzureDacpacDeployment@1  
    ![deplodb](https://github.com/tingkelvin/DeviceReg/assets/49113121/95b894d7-458a-4281-a127-c3879737299d)
 1. Build App Stage - DotNetCoreCLI@2 & ArchiveFiles@2  
    ![buildapp](https://github.com/tingkelvin/DeviceReg/assets/49113121/a2dec7c7-8642-4980-a47c-6b9cd2e4478f)
 1. Deploy App Stage - AzureFunctionApp@1  
    ![deployapp](https://github.com/tingkelvin/DeviceReg/assets/49113121/1de589cd-7ed1-4eb5-8ac2-39da0de3b6d6)
 1. Test Stage -  UsePythonVersion@0 & PublishTestResults@2  
    ![test](https://github.com/tingkelvin/DeviceReg/assets/49113121/5646ba1f-548f-4ae0-8a23-1043bab4b687)

Any commits to the main branch will trigger this pipeline.  
![AzurePipeline](https://github.com/tingkelvin/DeviceReg/assets/49113121/4e7ebc69-a6f2-42fc-82a9-8309ed004a02)

## Integration Test 

The test is done by Pytest, a testing frame work built in Python. The test will stimulate register request concurrently to the end point for registration.

### Test 1
This test will stimulate 10 requests to register one single device to the end points.
The purpose of the test is to see if get request to retrieve asset id is working.
This can be assessed by asserting response code is 200

    assert(res.status_code == 200)

Any response code rather than 200, indicates failure of the test.

### Test 2

This test is similar to previous test . 10 requests contain multiple devices registration are made to the end points.
The purpose of the test is to see if post request to retrieve multiple asset id is working.
This can be assessed by asserting response code is 200

    assert(res.status_code == 200)

Any response code rather than 200, indicates failure of the test.

### Test 3

This test is a more comprehensive one. 

The script will execute "DELETE FROM [dbo].[devices]" to the SQL database to clean the database.

Then, the script will read the Sample_Request.json and create a mix of single device registration and multiple device registration to the end point. 

The multiple device registration will contain a number of devices between 1 to 50.

The script will continue send request until we have sent all the request for 1000 devices. Every devices will only have one registration request sent.

Frist, we assessed the retry mechanism
This can be assessed by asserting response code is always 200

    assert(res.status_code == 200)

Secondly, we assessed if the total number of registration is 1000 and every request has a unique device id.

     assert(len(testDevices) == max_number and len(testDevices) == len(set(deviceIds)))

Thirdly, the duration of this test should be less than 10,

     assert(end - start < 10*60)

Lastly, the script will execute a query to database to retrieve all the deviceIds, and we check if every request has written the device id to the database.

     assert(deviceIds == queryDeviceIds)
     
 ### Publish Test Result
 Pytest could publish test summary and result in xml, which Azure DevOps can display in html.

 #### Test Summary
 ![testresult](https://github.com/tingkelvin/DeviceReg/assets/49113121/137ebcd7-a8bd-4b16-a7f6-1dc712022470)

 #### Test Cases
 ![testcase](https://github.com/tingkelvin/DeviceReg/assets/49113121/45addcd2-f7ab-471a-a112-b58d9e008cc1)

 #### Database Query
 ![sql](https://github.com/tingkelvin/DeviceReg/assets/49113121/3943ea2d-4f33-47e8-8380-2560e500d474)
<!-- USAGE EXAMPLES -->

## Conclusion

This was a very fun project to work. This is my first time to build a .Net application, Azure DevOps pipeline and different Azure Products.

### Things that I could do better

The CI/CD integration was extremely useful, which I should built it at the beginning of the project. At the begining, I was using python to build the Azure Function, which was working fine on my computer. However, when I tried to deploy to Azure Function, i notice that Azure Function has not supportted m1 python yet. If I built the pipeline at the start, I would have catch up this error earlier. In general, CI/CD and infrustructure as code tend to increase in speed of deployments, reduce errors and improve infrastructure consistency.

A separate trigger for automated deployment would be nice. When I was making changes to the SQL data configuration. This will trigger main branch and start the whole automated deployment. There is no need to wait for the .Net application to build and deploy.

Thread process may be implmented in Azure Function to speed up the application.

Some of the commit messages are not clear and duplicate, shoud have used commit --amend.
