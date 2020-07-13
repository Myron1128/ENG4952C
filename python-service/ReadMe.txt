These are the Python services for the Raspberry. 
Need to follow steps from this tutorial to make it work:
https://blog.iamlevi.net/2017/05/control-raspberry-pi-android-bluetooth/
Don't copy the raspibtsrv.py or raspibtsrv.service from that link. Copy them from this repository.
Additionally, you will probably need to replace your _init_.py file located in "/usr/local/lib/python2.7/dist-packages/bluetooth/__init__.py" with the one that is in this repository.
