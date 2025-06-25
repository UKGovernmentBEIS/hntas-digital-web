docker buildx install
docker buildx create --name mybuilder
docker buildx use mybuilder
docker buildx build -f HNTAS.Web.UI/Dockerfile -t hntas-armv7 . --platform linux/amd64,linux/arm/v7