# RunUO Dockerfiles

For builds uses:

1. RunUO release 2.7 (https://github.com/runuo/runuo/archive/refs/tags/v2.7.tar.gz)

2. fix from this issue (runuo#50)

3. neruns distro scripts (https://github.com/nerun/runuo-nerun-distro/releases/download/153/NerunsDistro-r153.zip)

Need set environment variable UOPath and mount volumes /data (volume for server files), /UO (volume with UO client files)


## Dockerfile.runuo:

for localhost installation and playing in local network (or in internet on VPC). Uses host network.

    docker run -it --name runuo \
    --net=host \
    --cap-add=NET_ADMIN \
    -e UOPath=/UO \
    -p 2593:2593 \
    -p 12000:12000 \
    -v $(pwd)/data:/data \
    -v $(pwd)/UO:/UO \
    runuo

First startup should be interactive for setting owner account. After recommend login to server and create owner account and save server configuration. (or that will autosaved in 5 minutes automatically).

## Dockerfile.runuo_zerotier:

for playing in internet throught Zerotire switch (https://my.zerotier.com/).

	docker run -it --name runuo_zerotier \
    --cap-add=NET_ADMIN \
    -e UOPath=/UO \
    -e netId=<you_zerotier_network_id> \
    --device=/dev/net/tun \
    --cap-add=SYS_ADMIN \
    --cap-add=SYS_RESOURCE \
    -v $(pwd)/data:/data \
    -v $(pwd)/UO:/UO \
    runuo_zerotier

additionally need set netId env var.

## Dockerfile.runuo_i2p:

for playing throught i2p

    docker run -it --name runuo_i2p \
    --cap-add=NET_ADMIN \
    -v $(pwd)/data:/data \
    -v $(pwd)/UO:/UO \
    runuo_i2p

During first startup will created i2p hostname which need save for next using in client tunnels.conf

My client conf is:

    [runuo]
    type = client
    address = 127.0.0.1
    port = 2593
    destination = iwgbbjqgd5tlfbd63a67ytwnpz337j7ktmdznsvfpnmntpuhhoyq.b32.i2p
    destinationport = 2593



