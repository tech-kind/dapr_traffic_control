docker build -t dapr-trafficcontrol/mosquitto:1.0 .
docker run -d -p 1883:1883 -p 9001:9001 --name dtc-mosquitto dapr-trafficcontrol/mosquitto:1.0
