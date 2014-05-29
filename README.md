###Overview
ElasticSearch is a powerful open source search and analytics engine that makes data easy to explore. 

This Brewmaster template sets up a ElasticSearch cluster with minimum effort on Windows Azure.

###What this Brewmaster Template achieves
1. Downloads and installs JRE and ElasticSearch.
2. Virtual Machines are created using the latest version of Windows Server.
3. ElasticSearch is installed as a service and the start up mode is set to 'Automatic'.
4. Azure Cloud Plugin for Elasticsearch is installed to facilitate auto discovery.
5. Elasticsearch Head Plugin is installed to provide a dashboard.
6. Firewall rules are addded to open ports 9200 and 9300.
7. Azure Cloud Plugin is configured for auto discovery of nodes.
8. A 100 GB disk is created and attached to the VM, for storing ElasticSearch data.

###Certificate Configuration
- The Azure Cloud Plugin for ElasticSearch needs access to the Azure management APIs to discover nodes. For this it needs a certificate that can grant it access.
- To create a certificate run the following command </br>
`makecert -sky elasticsearch -r -n "CN=elasticsearchcert" -pe -a sha1 -len 2048 -ss My "ElasticSearchCert.cer"`
- This will create a ElasticSearchCert.cer(public key) and the certificate is installed in your certificate store (Personal > Certificates) which you can view using certmgr.msc 
- You need to upload the ElasticSearchCert.cer file to the Azure Portal > Settings > Management Certificates
- Next you need to export the certificate along with the private key from your certificate store using certmgr.msc
- Ensure you select the export private key option and secure it using a password. The exported file type has to be .pfx
- Upload this .pfx file to any web location so that Brewmaster can download it via a http/https url.
- The url to the .pfx file has to be provided as the AzureCertificateUrl parameter for the template, along with the password for that file.

###Terms of use
- [JRE](http://www.oracle.com/technetwork/java/javase/terms/license/index.html)
- [ElasticSearch](http://www.elasticsearch.org/terms-of-use/)
- [ElasticSearch Head](https://github.com/mobz/elasticsearch-head/blob/master/LICENCE)
- [Azure Cloud Plugin for ElasticSearch](https://github.com/elasticsearch/elasticsearch-cloud-azure/blob/master/LICENSE.txt)

###References
Please refer to the following links for more information.
> - [Elastic Search](http://www.elasticsearch.org/)
> - [Azure Cloud Plugin for Elasticsearch](https://github.com/elasticsearch/elasticsearch-cloud-azure)
> - [elasticsearch-head](https://github.com/mobz/elasticsearch-head)
> - [Server JRE](http://www.oracle.com/technetwork/java/javase/downloads/server-jre8-downloads-2133154.html)
