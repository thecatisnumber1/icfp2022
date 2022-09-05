import bestof
import re

mapping = {
26: 5,
27: 2,
28: 10,
29: 18,
30: 11,
31: 24,
32: 9,
33: 15,
34: 7,
35: 25,
36: 17,
37: 16,
#40: 5,
}

inv_mapping = {mapping[i]:[i] for i in mapping}
inv_mapping[5].append(40)



def repl(id, base):
  parts = map(int, id.split('.'))
  parts[0] += base
  return '.'.join(map(str, parts))

def find(reg, line):
  parts = re.findall(reg, line)
  return parts and parts[0]

def enrich(blocky, notblocky):
  block = bestof.get_file(bestof.buckets[blocky]['id'])
  canvas = bestof.get_file(bestof.buckets[notblocky]['id'])
  #block = open('27.ins', 'r').read()
  #canvas = open('2.ins', 'r').read()
  blines = block.split('\n')
  for i,line in enumerate(blines):
    if not line.startswith('merge'): break
  
  base = int(re.findall('\[(\d+)\]', line)[0])
  merge = blines[:i]
  acc = []

  clines = canvas.split('\n')
  if clines[0].startswith('color') and clines[1].startswith('color'):
    clines = clines[1:]

  if not clines[0].startswith('color') and merge:
    merge.append('color [%d] [255, 255, 255, 255]'%(base))

  for line in clines:
    parts = find('cut \[([0-9\.]+)\] \[(.)\] \[(\d+)\]', line)
    if parts:
      merge.append('cut [%s] [%s] [%s]'%(repl(parts[0], base), parts[1], parts[2]))
      continue
    parts = find('cut \[([0-9\.]+)\] \[(\d+, \d+)\]', line)
    if parts:
      merge.append('cut [%s] [%s]'%(repl(parts[0], base), parts[1]))
      continue
    parts = find('color \[([0-9\.]+)\] \[(\d+, \d+, \d+, \d+)\]', line)
    if parts:
      merge.append('color [%s] [%s]'%(repl(parts[0], base), parts[1]))
      continue
    parts = find('merge \[([0-9\.]+)\] \[([0-9\.]+)\]', line)
    if parts:
      merge.append('merge [%s] [%s]'%(repl(parts[0], base), repl(parts[1], base)))
      continue
    print line
    1/0

  last_color = -1
  for i in xrange(len(merge)):
    if merge[i].startswith('color'):
      last_color = i

  rez = '\n'.join(merge[:last_color+1])
  print bestof.recent[blocky]['score'], blocky, notblocky
  print bestof.posts(blocky, rez).text

rez = bestof.get_all()
bestof.fill_buckets(rez)
last = int(open('latest').read())

probs = set([i['problem_id'] for i in rez['submissions'] if i['id'] > last and i['status'] == 'SUCCEEDED'])

print probs
for i in sorted(probs):
  if i not in inv_mapping: continue
  for j in inv_mapping[i]:
    enrich(j, i)

rez['submissions']

ids = [i['id'] for i in rez['submissions'] if i['status'] == 'SUCCEEDED']
print ids[0]
open('latest', 'w').write(str(ids[0]))
