
from multiprocessing import Process

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
