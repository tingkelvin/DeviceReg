from multiprocessing import Process

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