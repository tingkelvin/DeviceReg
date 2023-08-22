
from multiprocessing import Process
import pytest
import requests
# import pyodbc





def post_request(host):
  url = f"http://{host}/api/register"
  headers = {"Content-Type": "application/json"}
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
  res = requests.post(url, headers=headers, json=data)
  assert(res.status_code == 200)

def test_function_with_scenario_one(host):
  processPool = []
  for i in range(10):
    process = Process(target=post_request, args=(host,))
    processPool.append(process)
    process.start()
  for process in processPool:
    process.join()
  print("done")



