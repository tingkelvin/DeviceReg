
from multiprocessing import Process
import requests
# import pyodbc

url = "http://localhost:7071/api/register"

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

def post_request(data):
  print("start thread")
  res = requests.post(url, headers=headers, json=data)
  print(res.status_code)
  assert(res.status_code == 200)

def test_function_with_scenario_one():
  processPool = []
  for i in range(10):
    process = Process(target=post_request, args = (data,))
    processPool.append(process)
    process.start()
  
  for process in processPool:
    process.join()
  
  print("done")



