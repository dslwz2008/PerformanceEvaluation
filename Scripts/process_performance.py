# coding:utf-8

import io
import psutil
import time
from datetime import datetime

# get from taskmgr
target_pid = 13188

# Example Data:
# {'以太网': snetio(bytes_sent=2693730598, bytes_recv=17211723862, packets_sent=4454361, packets_recv=0, errin=0, errout=0, dropin=0, dropout=0),
# '本地连接* 4': snetio(bytes_sent=0, bytes_recv=0, packets_sent=0, packets_recv=0, errin=0, errout=0, dropin=0, dropout=0),
# 'VMware Network Adapter VMnet1': snetio(bytes_sent=4393, bytes_recv=22, packets_sent=19, packets_recv=22, errin=0, errout=0, dropin=0, dropout=0),
# 'VMware Network Adapter VMnet8': snetio(bytes_sent=6511, bytes_recv=23, packets_sent=19, packets_recv=23, errin=0, errout=0, dropin=0, dropout=0),
# 'WLAN': snetio(bytes_sent=137610357, bytes_recv=1970745088, packets_sent=886502, packets_recv=1426192, errin=0, errout=0, dropin=0, dropout=0),
# 'Loopback Pseudo-Interface 1': snetio(bytes_sent=0, bytes_recv=0, packets_sent=0, packets_recv=0, errin=0, errout=0, dropin=0, dropout=0),
# '本地连接* 14': snetio(bytes_sent=0, bytes_recv=0, packets_sent=0, packets_recv=0, errin=0, errout=0, dropin=0, dropout=0)}
target_net = '以太网'

def main():
    # get process
    for proc in psutil.process_iter():
        if proc.pid == target_pid:
            target_proc = proc
            filename = target_proc.name() + '.csv'

    fp = open(filename, 'w')
    fp.write('Datetime,CPU Percent,Memory Size,Network Sent,Network Received\n')

    # init net valus
    netio = psutil.net_io_counters(pernic=True)[target_net]
    last_sent_bytes = netio.bytes_sent
    last_recv_bytes = netio.bytes_recv

    try:
        while target_proc is not None:
            cpu_pcnt = target_proc.cpu_percent() / psutil.cpu_count()
            mem_usage = target_proc.memory_info().rss/2**20
            netio = psutil.net_io_counters(pernic=True)[target_net]
            cur_sent_bytes = netio.bytes_sent
            cur_recv_bytes = netio.bytes_recv
            sent_bytes = cur_sent_bytes - last_sent_bytes
            recv_bytes = cur_recv_bytes - last_recv_bytes
            full_record = '{0},{1},{2},{3},{4}\n'.format(datetime.now().isoformat(),cpu_pcnt,mem_usage,sent_bytes,recv_bytes)
            fp.write(full_record)
            last_sent_bytes = cur_sent_bytes
            last_recv_bytes = cur_recv_bytes
            time.sleep(0.3)
    # 进程已关闭
    except psutil.NoSuchProcess:
        fp.flush()
        fp.close()
        print('Process Exit!')
    # 按键退出
    except KeyboardInterrupt:
        fp.flush()
        fp.close()
        print('Keyboard Interrupt!')


if __name__ == '__main__':
    main()

