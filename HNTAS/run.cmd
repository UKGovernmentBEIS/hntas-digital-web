docker stop hntas
docker rm hntas
docker run --name hntas --detach --publish 8080:8080 hntas