
from multiprocessing import Process
import pytest
import requests
# import pyodbc




def test_function_with_scenario_one(utils):
  processPool = []
  for i in range(40):
    process = Process(target=utils.post_request)
    processPool.append(process)
    process.start()
  for process in processPool:
    process.join()
  print("done")



