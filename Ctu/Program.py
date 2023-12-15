import pyodbc
import pathlib

def readFromFiles() -> None:
    fileNames = []
    for i in range(1, 32):
        fileNames.append("TC1000" + str(i).zfill(2) + ".txt")
    files = []
    conn = getDB()
    cursor = conn.cursor()
    for fileName in fileNames:
        # check file modify time
        filePath = "C:/WangPeng/Ctu/Enterprise/Data/" + fileName
        fileModify = str(pathlib.Path(filePath).stat().st_mtime)
        print(filePath)
        print(fileModify)
        # search file in DB to check whether import that file already
        cursor.execute(f"select * from SysPunchFile where FileName='{fileName}' and ModifyTime='{fileModify}'")
        row = cursor.fetchone()
        if row:
            continue
        # print(f"file name: {filePath} modify time: {fileModify}")
        # with open("//172.16.20.21/Enterprise Suite/Enterprise/Data/" + fileName, "r") as f:
        with open(filePath, "r") as f:
            data = f.readlines()
            files.append([data, fileName, fileModify])
            # for line in data:
            #     print(line.strip())
    cursor.close()
    conn.close()
    return files

def getDB():
    SERVER = 'localhost'
    DATABASE = 'ShiftSchedule'
    USERNAME = 'sa'
    PASSWORD = 'Btrust123'
    connectionString = f'DRIVER={{SQL Server}};SERVER={SERVER};DATABASE={DATABASE};UID={USERNAME};PWD={PASSWORD}'
    conn = pyodbc.connect(connectionString)
    return conn

def insertDB(lines: list[str], fileName: str, fileModify) -> None:
    items = len(lines)
    if not items:
        return
    data = lines[0].strip('\n').split(",")
    firstYear, firstMonth, firstDay, firstHour, firstMinute = data[10].strip('\n'), data[8].strip(), data[9].strip(), data[6].strip(), data[7].strip()
    try:
        conn = getDB()
        cursor = conn.cursor()
        
        # check whether import this file already
        cursor.execute(f"select * from SysPunchFile where FileName='{fileName}' and Items={items} and FirstYear='{firstYear}' and FirstMonth='{firstMonth}' and FirstDay='{firstDay}' and FirstHour='{firstHour}' and FirstMinute='{firstMinute}'")
        row = cursor.fetchone()
        if row:
            return
        
        for i, line in enumerate(lines):
            line = line.strip('\n')
            data = line.split(",")
            if i == 0:
                firstYear, firstMonth, firstDay, firstHour, firstMinute = data[10].strip('\n'), data[8].strip(), data[9].strip(), data[6].strip(), data[7].strip()
            sql = "INSERT INTO SysPunch(HandRec, SiteNumber, ClockNumber, ClockCode, BtrustId, WorkCode, Hour, Minute, Month, Day, Year) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)"
            cursor.execute(sql, *data)
        # insert in to DB of file modify time
        sql = f"Merge into SysPunchFile as t using (select '{fileName}' as FileName, '{items}' as Items) as s on s.FileName = t.FileName when not matched then insert values( '{fileName}', '{fileModify}', {items}, '{firstYear}', '{firstMonth}', '{firstDay}', '{firstHour}', '{firstMinute}') when matched then update set ModifyTime = '{fileModify}', items={items}, firstYear='{firstYear}', firstMonth='{firstMonth}', firstDay='{firstDay}', firstHour='{firstHour}', firstMinute='{firstMinute}';"
        cursor.execute(sql)
        conn.commit()
    finally:
        cursor.close()
        conn.close()
    

if __name__ == "__main__":
    files = readFromFiles()
    for i, file in enumerate(files):
        [data, fileName, fileModify] = file
        insertDB(data, fileName, fileModify)
