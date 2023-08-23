from multiprocessing import Process
import random
import time

def test_three_random_mix_request(host, utils, devicesData, server:str, database:str, username:str, password:str, driver:str):
  print("Starting random mix request test")
  utils.clean_database(server=server, database=database, username=username, password=password, driver=driver)
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
  queryDeviceIds = utils.sql_query(query="SELECT DeviceId FROM [dbo].[devices]", 
                                   server=server, 
                                   database=database, 
                                   username=username, 
                                   password=password, 
                                   driver=driver)
  queryDeviceIds.sort()
  deviceIds.sort()
  assert(deviceIds == queryDeviceIds)

  print("Random mix request test done")