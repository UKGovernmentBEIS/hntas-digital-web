docker stop hntas-digital
docker rm hntas-digital
docker run --name hntas-digital --detach --publish 8080:8080 hntas-digital

