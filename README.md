###Overview
ElasticSearch is a powerful open source search and analytics engine that makes data easy to explore. 

This Brewmaster template sets up a ElasticSearch cluster with minimum effort on Windows Azure.

###What this Brewmaster Template achieves
1. Downloads and installs JRE and ElasticSearch
2. Virtual Machines are created using the latest version of Windows Server.
3. ElasticSearch is installed as a service and the start up mode is set to 'Automatic'
4. Azure Cloud Plugin for Elasticsearch is installed to facilitate auto discovery.
5. Elasticsearch Head Plugin is installed to provide a dashboard.
6. Firewall rules are addded to open ports 9200 and 9300

###Note
This template uses a cached copy of JRE, please ensure you read the [JRE usage terms] (http://www.oracle.com/technetwork/java/javase/terms/license/index.html)

###References
Please refer to the following links for more information.
> - [Elastic Search](http://www.elasticsearch.org/)
> - [Azure Cloud Plugin for Elasticsearch](https://github.com/elasticsearch/elasticsearch-cloud-azure)
> - [elasticsearch-head](https://github.com/mobz/elasticsearch-head)
> - [Server JRE] (http://www.oracle.com/technetwork/java/javase/downloads/server-jre8-downloads-2133154.html)
