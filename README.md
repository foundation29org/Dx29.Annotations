<div style="margin-bottom: 1%; padding-bottom: 2%;">
	<img align="right" width="100px" src="https://dx29.ai/assets/img/logo-Dx29.png">
</div>

Dx29 Annnotations
==============================================================================================================================================

Dx29.Annotations [![Build Status](https://f29.visualstudio.com/Dx29%20v2/_apis/build/status/DEV-MICROSERVICES/Dx29.Annotations?branchName=develop)](https://f29.visualstudio.com/Dx29%20v2/_build/latest?definitionId=68&branchName=develop)

Dx29.Annotations-Jobs [![Build Status](https://f29.visualstudio.com/Dx29%20v2/_apis/build/status/DEV-MICROSERVICES/Dx29.Annotations-Jobs?branchName=develop)](https://f29.visualstudio.com/Dx29%20v2/_build/latest?definitionId=69&branchName=develop)

### **Overview**

This project is used to extract information from medical documents.

It is used in the [Dx29 application](https://dx29.ai/) and therefore how to integrate it is described in the [Dx29 architecture guide](https://dx29-v2.readthedocs.io/en/latest/index.html).

It is programmed in C#, and the structure of the project is as follows:

>- src folder: This is made up of multimple folders which contains the source code of the project.
>>- **Dx29.Annotations.Web.API**. In this project is the implementation of the controllers that expose the aforementioned methods.
>>- **Dx29.Annotations**. It is this project that contains the logic to perform the relevant operations: services and communication with the external NCR and Text Analytics apis.
>>- **Dx29.Annotations.Worker**. This project manages the Dispatcher required for the asynchronous functionalities. The administration and communication with the ServiceBus is carried out in this project.
>>- **Dx29**, **Dx29.Azure** and **Dx29.Jobs** used as libraries to add the common or more general functionalities used in Dx29 projects programmed in C#.
>- .gitignore file
>- README.md file
>- manifests folder: with the YAML configuration files for deploy in Azure Container Registry and Azure Kubernetes Service.
>- pipeline sample YAML file. For automatizate the tasks of build and deploy on Azure.

<p>&nbsp;</p>

### **Getting Started**

####  1. Configuration: Pre-requisites

The project must include a configuration file: appsettings.json. This includes the dependencies with other microservices:

|  Key           | Value     |		                                                         |
|----------------|-----------|---------------------------------------------------------------|
| FileStorage    | Endpoint  |http://dx29-filestorage/api/v1/                                |
| MedicalHistory | Endpoint  |http://dx29-medicalhistory/api/v1/                             |
| Segmentation   | Endpoint  |http://dx29-segmentation:8080/api/v1/                          |
| DocConverter   | Endpoint  |https://f29api.northeurope.cloudapp.azure.com/api/             |

And finally, it should be mentioned that:
>- It depends on a external service: TextAnalytics
>- It accesses the blob 
>- It is necessary to create a ServiceBus, where the asynchronous jobs will be sent, SignalR for notification management, and AppInsights for logging. 

Therefore, in order to run it, the file appsettings.secrets.json must be added to the secrets folder with the following information:

|  Key                 | Value               |		                                                                                |
|----------------------|---------------------|--------------------------------------------------------------------------------------|
| ConnectionStrings    | BlobStorage         |Blob endpoint and credentials                                                         |
| ServiceBus           | ConnectionString    |Connection string service bus                                                         |
| ServiceBus           | QueueName           |Queue configured name                                                                 |
| SignalR              | ConnectionString    |SignalR connection string & credentials                                               |
| SignalR              | HubName             |SignalR Hub HubName                                                                   |
| AppInsights          | Key                 |Secret key for connecting with AppInsights                                            |
| CognitiveServices    | Endpoint            |Endpoint Azure cognitive service configured                                           |
| CognitiveServices    | Authorization       |Authorization key                                                                     |
| CognitiveServices    | Region              |Azure cognitive service region configured                                             |
| TAHAnnotation        | Endpoint            |Endpoint Azure cognitive service configured  for Text Analytics                       |
| TAHAnnotation        | Path                |text/analytics/v3.1/entities/health/jobs                                              |
| TAHAnnotation        | Authorization       |Authorization key                                                                     |
| TAHAnnotation        | BlackList           |User ids in black list                                                                |

<p>&nbsp;</p>

####  2. Download and installation

Download the repository code with `git clone` or use download button.

We use [Visual Studio 2019](https://docs.microsoft.com/en-GB/visualstudio/ide/quickstart-aspnet-core?view=vs-2022) for working with this project.

<p>&nbsp;</p>

####  3. Latest releases

The latest release of the project deployed in the [Dx29 application](https://dx29.ai/) is: 
>- Dx29.Annotations: v0.15.02.
>- Dx29.AnnotationsJobs: v0.15.02.

<p>&nbsp;</p>

#### 4. API references

>- Process. 
>>- To make external queries to Dx29, that is to say, with documents that are not associated to any user profile of the application.
>>- POST request: ```/api/v1/Annotations/process```
>>- Body request: Stream document.
>>- Response: Job Status
>>>- Name
>>>- Token: Job identifier. This will be needed later for job status and/or result queries.
>>>- Date: Date when the query is performed
>>>- Status: String indicating the status of the job. Can be: Failed, Succeeded, Running, Preparing, Pending, Created or Unknown.
>>>- CreatedOn: Date when the operation has made.
>>>- LastUpdate
>- Process/userId/caseId/reportId. 
>>- For internal Dx29 queries, i.e. on users, cases and existing reports. This request will query the blobs to perform the relevant operations on the indicated document (For the user and the case, the report with identifier reportId is searched, and processed).
>>- POST request: ```/api/v1/Annotations/process/{userId}/{caseId}/{reportId}```
>>- Body request: Stream document.
>>- Response: Job status. An object like the one in the request described above is returned.
>- Status. 
>>- To request the status of a given job. The identifier of the job corresponds to the token returned by any of the previous requests.
>>- GET request: ```/api/v1/Annotations/status?params=<token>```
>>- Response: Job status. An object like the one in the request described above is returned.
>- Results.
>>- To obtain the results of a given job. The job identifier corresponds to the token returned by any of the previous requests. 
>>- GET request: ```/api/v1/Annotations/results?params=<token>```
>>- Object with the results of the annotation process:
>>>- Analyzer: If the document wa analyze with OCR or not.
>>>- Segments List. List of the segments extracted as a result, composed by objects with:
>>>>- Id or identifier of the segment
>>>>- Language of the results
>>>>- String Text
>>>>- Source composed by the Language and the string text sources (from the document).
>>>>- Annotations list or the items that are recognised in the result text:
>>>>>- Id or identifier
>>>>>- Text of the annotation
>>>>>- Offset
>>>>>- Length
>>>>>- Category
>>>>>- CondifenceScore
>>>>>- IsNegated
>>>>>- IsDiscarded
>>>>>- List of related links composed by the data of the source and an Id.

<p>&nbsp;</p>

### **Build and Test**

#### 1. Build

We could use Docker. 

Docker builds images automatically by reading the instructions from a Dockerfile – a text file that contains all commands, in order, needed to build a given image.

>- A Dockerfile adheres to a specific format and set of instructions.
>- A Docker image consists of read-only layers each of which represents a Dockerfile instruction. The layers are stacked and each one is a delta of the changes from the previous layer.

Consult the following links to work with Docker:

>- [Docker Documentation](https://docs.docker.com/reference/)
>- [Docker get-started guide](https://docs.docker.com/get-started/overview/)
>- [Docker Desktop](https://www.docker.com/products/docker-desktop)

The first step is to run docker image build. We pass in . as the only argument to specify that it should build using the current directory. This command looks for a Dockerfile in the current directory and attempts to build a docker image as described in the Dockerfile. 
```docker image build . ```

[Here](https://docs.docker.com/engine/reference/commandline/docker/) you can consult the Docker commands guide.

<p>&nbsp;</p>

#### 2. Deployment

To work locally, it is only necessary to install the project and build it using Visual Studio 2019. 

The deployment of this project in an environment is described in [Dx29 architecture guide](https://dx29-v2.readthedocs.io/en/latest/index.html), in the deployment section. In particular, it describes the steps to execute to work with this project as a microservice (Docker image) available in a kubernetes cluster:

1. Create an Azure container Registry (ACR). A container registry allows you to store and manage container images across all types of Azure deployments. You deploy Docker images from a registry. Firstly, we need access to a registry that is accessible to the Azure Kubernetes Service (AKS) cluster we are creating. For this purpose, we will create an Azure Container Registry (ACR), where we will push images for deployment.
2. Create an Azure Kubernetes cluster (AKS) and configure it for using the prevouos ACR
3. Import image into Azure Container Registry
4. Publish the application with the YAML files that defines the deployment and the service configurations. 

This project includes, in the Deployments folder, YAML examples to perform the deployment tasks as a microservice in an AKS. 

Note that this service is configured as "ClusterIP" since it is not exposed externally in the [Dx29 application](https://dx29.ai/), but is internal for the application to use. If it is required to be visible there are two options:
>- The first, as realised in the Dx29 project an API is exposed that communicates to third parties with the microservice functionality.
>- The second option is to directly expose this microservice as a LoadBalancer and configure a public IP address and DNS.

**Interesting link**: [Deploy a Docker container app to Azure Kubernetes Service](https://docs.microsoft.com/en-GB/azure/devops/pipelines/apps/cd/deploy-aks?view=azure-devops&tabs=java)

<p>&nbsp;</p>


### **Contribute**

Please refer to each project's style and contribution guidelines for submitting patches and additions. The project uses [gitflow workflow](https://nvie.com/posts/a-successful-git-branching-model/). 
According to this it has implemented a branch-based system to work with three different environments. Thus, there are two permanent branches in the project:
>- The develop branch to work on the development environment.
>- The master branch to work on the test and production environments.

In general, we follow the "fork-and-pull" Git workflow.

>1. Fork the repo on GitHub
>2. Clone the project to your own machine
>3. Commit changes to your own branch
>4. Push your work back up to your fork
>5. Submit a Pull request so that we can review your changes

The project is licenced under the **(TODO: LICENCE & LINK & Brief explanation)**

<p>&nbsp;</p>
<p>&nbsp;</p>

<div style="border-top: 1px solid !important;
	padding-top: 1% !important;
    padding-right: 1% !important;
    padding-bottom: 0.1% !important;">
	<div align="right">
		<img width="150px" src="https://dx29.ai/assets/img/logo-foundation-twentynine-footer.png">
	</div>
	<div align="right" style="padding-top: 0.5% !important">
		<p align="right">	
			Copyright © 2020
			<a style="color:#009DA0" href="https://www.foundation29.org/" target="_blank"> Foundation29</a>
		</p>
	</div>
<div>
