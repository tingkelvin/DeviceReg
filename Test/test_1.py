
from multiprocessing import Process
import pytest
import requests
# import pyodbc




def test_one_100_get_request(host, utils):
  processPool = []
  data = {
    "devices": [
        {
            "id":"DVID000001",
            "Name": "device 1 Name",
            "location":"Here",
            "type": "1"
        }
    ]
  }
  for i in range(100):
    process = Process(target=utils.post_request, args=(host,data,"Get-Request", i+1))
    processPool.append(process)
    process.start()
  for process in processPool:
    process.join()
  print("Get request test done")

def test_two_100_post_request(host, utils):
  processPool = []
  data = {
    "correlationId": "84f84dc5-d0a7-440f-92e0-c926ad8aa709",
    "devices": [
        {
            "id": "DVID000004",
            "Name": "device 1 name",
            "location": "location 1",
            "type": "device type 1"
        },
        {
            "id": "DVID000005",
            "Name": "device 2 name",
            "location": "location 2",
            "type": "device type 2"
        },
        {
            "id": "DVID000006",
            "Name": "device 3 name",
            "location": "location 3",
            "type": "device type 3"
        }
    ]
  }
  # utils.post_request(host,data)
  # process = Process(target=utils.post_request, args=(host,data))
  for i in range(10):
    process = Process(target=utils.post_request, args=(host,data,"Post-Request", i+1))
    processPool.append(process)
    process.start()
  for process in processPool:
    process.join()
  print("Post request test done")



