
import requests
import pyodbc

url = "https://devicereg.azurewebsites.net/api/register"

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


def test_function_with_scenario_one():
  res = requests.post(url, headers=headers, json=data)
  assert(res.status_code == 200)

