import pytest
import requests
import json

def pytest_addoption(parser):
    parser.addoption("--host", action="store", default="localhost:7071")

def pytest_generate_tests(metafunc):
    # This is called for every test. Only get/set command line arguments
    # if the argument is specified in the list of test "fixturenames".
    option_value = metafunc.config.option.host
    if 'host' in metafunc.fixturenames and option_value is not None:
        metafunc.parametrize("host", [option_value])
        
class Utils:
    @staticmethod
    def post_request(host,data,testname, testNo):
        url = f"http://{host}/api/register"
        headers = {"Content-Type": "application/json"}
        res = requests.post(url, headers=headers, json=data)
        print(f"{testname}-{testNo}: {res.status_code}")
        assert(res.status_code == 200)
        

@pytest.fixture
def utils():
    return Utils

@pytest.fixture
def devicesData():
    return json.load(open("Sample_Request.json"))
    