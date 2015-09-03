# Mq
Tools for export/import message in msqueue

## Export

To export all messages from dedicated queue
```
mq -e localhost myqueue 
```

To export only 1 message from dedicated queue
```
mq -e localhost myqueue -m 1
```

To export all messages from transactional queue
```
mq -i localhost myqueue 
```

## Import

To import file into msqueue
```
mq -i localhost myqueue tmp45.tmp
```

To import files into msqueue
```
mq -i localhost myqueue tmp45.tmp tmp46.tmp tmp47.tmp
```

To import file pattern into msqueue
```
mq -i localhost myqueue *.tmp
```

To import file into transactional queue
```
mq -i localhost myqueue tmp45.tmp -t
```
