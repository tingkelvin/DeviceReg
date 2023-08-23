import pytest
import requests
import json
import pyodbc

def pytest_addoption(parser):
    parser.addoption("--host", action="store", default="localhost:7071")
    parser.addoption("--server", action="store", type = str)
    parser.addoption("--username", action="store")
    parser.addoption("--password", action="store")
    parser.addoption("--database", action="store")
    parser.addoption("--driver", action="store", default="{ODBC Driver 17 for SQL Server}")

def pytest_generate_tests(metafunc):
    # This is called for every test. Only get/set command line arguments
    # if the argument is specified in the list of test "fixturenames".
    option_host = metafunc.config.option.host
    option_server = metafunc.config.option.server
    option_database = metafunc.config.option.database
    option_username = metafunc.config.option.username
    option_password = metafunc.config.option.password
    option_driver = metafunc.config.option.driver
    if 'host' in metafunc.fixturenames and option_host is not None:
        metafunc.parametrize("host", [option_host])
    if 'server' in metafunc.fixturenames and option_server is not None:
        metafunc.parametrize("server", [option_server])
    if 'database' in metafunc.fixturenames and option_database is not None:
        metafunc.parametrize("database", [option_database])
    if 'username' in metafunc.fixturenames and option_username is not None:
        metafunc.parametrize("username", [option_username])
    if 'password' in metafunc.fixturenames and option_password is not None:
        metafunc.parametrize("password", [option_password])
    if 'driver' in metafunc.fixturenames and option_driver is not None:
        metafunc.parametrize("driver", [option_driver])
        
class Utils:
    @staticmethod
    def post_request(host,data,testname, testNo):
        url = f"http://{host}/api/register"
        headers = {"Content-Type": "application/json"}
        res = requests.post(url, headers=headers, json=data)
        print(f"{testname}-{testNo}: {res.status_code}")
        assert(res.status_code == 200)

    def sql_query(query):
        server = 'deviceregappsql.database.windows.net'
        database = 'devicesdatabase'
        username = 'admin-sql'
        password = 'Abc!23321'   
        driver= '{ODBC Driver 17 for SQL Server}'
        ret = []
        with pyodbc.connect('DRIVER='+driver+';SERVER=tcp:'+server+';PORT=1433;DATABASE='+database+';UID='+username+';PWD='+ password) as conn:
            with conn.cursor() as cursor:
                cursor.execute(query)
                row = cursor.fetchone()
                while row:
                    ret.append(str(row[0]))
                    row = cursor.fetchone()
        return ret
    
    def clean_database(server):
        # server = 'deviceregappsql.database.windows.net'
        database = 'devicesdatabase'
        username = 'admin-sql'
        password = 'Abc!23321'   
        driver= '{ODBC Driver 17 for SQL Server}'
        with pyodbc.connect('DRIVER='+driver+';SERVER=tcp:'+server+';PORT=1433;DATABASE='+database+';UID='+username+';PWD='+ password) as conn:
            with conn.cursor() as cursor:
                cursor.execute("DELETE FROM [dbo].[devices]")
        
@pytest.fixture
def utils():
    return Utils

@pytest.fixture
def devicesData():
    return json.load(open("Sample_Request.json"))
    