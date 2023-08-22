
from multiprocessing import Process
import random
import time


# server = 'deviceregappsql.database.windows.net'
# database = 'devicesdatabase'
# username = 'admin-sql'
# password = 'Abc!23321'   
# driver= '{ODBC Driver 17 for SQL Server}'

def test_one_10_get_request(host, utils, devicesData):
  print("Starting get request test")
  processPool = []

  data = {
    "devices": [
      devicesData["devices"][0]
    ]
  }

  # generate 10 get request
  for i in range(10):
    process = Process(target=utils.post_request, args=(host,data,"Get Request Test", i+1))
    processPool.append(process)
    process.start()

  # make sure all the process finish
  for process in processPool:
    process.join()
  print("Get request test done")

def test_two_10_post_request(host, utils, devicesData):
  print("Starting post request test")
  processPool = []
  data = {
    "correlationId": "84f84dc5-d0a7-440f-92e0-c926ad8aa709",
    "devices": devicesData["devices"][:3]
    
  }

  # generate 10 post request
  for i in range(10):
    process = Process(target=utils.post_request, args=(host,data,"Post Request Test", i+i))
    processPool.append(process)
    process.start()
  
  # make sure all the process finish
  for process in processPool:
    process.join()
  print("Post request test done")

def test_three_random_mix_request(host, utils, devicesData, server, database, username, password, driver):
  print("Starting random mix request test")
  utils.clean_database(server, database, username, password, driver)
  start = time.time()
  testDevices = []
  processPool = []
  max_number = 1000
  i = 0

  # generate 1000 random request 
  while i < max_number:
    choice = random.randint(0, 1)
    if choice == 1:
      data = {
        "devices": [
          devicesData["devices"][i]
        ]
      }
      testDevices += [devicesData["devices"][i]]
      process = Process(target=utils.post_request, args=(host,data,"Get Request Test", i+i))
      i += 1
    else:
      index =  min(i + random.randint(10, 50), max_number)
      data = {
      "devices": devicesData["devices"][i:index]   
      }
      process = Process(target=utils.post_request, args=(host,data,"Post Request Test", i+1))
      testDevices += devicesData["devices"][i:index]
      i = index
    processPool.append(process)
    process.start()
  for process in processPool:
    process.join()

  # gather all the deviceId
  deviceIds = []
  for testDevice in testDevices:
    deviceIds.append(testDevice["id"])

  # assert all the test devices are tested
  assert(len(testDevices) == max_number and len(testDevices) == len(set(deviceIds)))

  # assert all request are done in 10 mins
  end = time.time()
  assert(end - start < 10*60)

  # assert all devices are written to sql database
  queryDeviceIds = utils.sql_query("SELECT DeviceId FROM [dbo].[devices]", server, database, username, password, driver)
  queryDeviceIds.sort()
  deviceIds.sort()
  assert(deviceIds == queryDeviceIds)

  print("Random mix request test done")

