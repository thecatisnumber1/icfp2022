import requests
import time

api_key = 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJlbWFpbCI6InRoZWNhdGlzbnVtYmVyMUBnb29nbGVncm91cHMuY29tIiwiZXhwIjoxNjYyMzczMjk3LCJvcmlnX2lhdCI6MTY2MjI4Njg5N30.dhJtIgf1DSzfJTK82DihkTL7expJRwgF6Akr0ixRR3Q'
api_key = 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJlbWFpbCI6InRoZWNhdGlzbnVtYmVyMUBnb29nbGVncm91cHMuY29tIiwiZXhwIjoxNjYyNDU5ODU3LCJvcmlnX2lhdCI6MTY2MjM3MzQ1N30.oLOwHGoWkXWqadEkQvjusk5E_ze8PbFHI7qAUkc2UyE'
headers = {
  'Authorization': 'Bearer ' + api_key,
}

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

def get_rez():
  return requests.get('https://robovinci.xyz/api/results/user', headers=headers).json()

def get_report():
  z = get_rez()
  acc = []
  for i in z['results']:
    prob = i['problem_id']
    minc = i['min_cost']
    best = i['overall_best_cost']
    acc.append((prob, minc, best, float(minc) / best))

  acc = sorted(acc, key=lambda x: x[3])
  for (prob, minc, best, rat) in acc:
    print ('%2daaaa%f %6d %6d'%(prob, rat, minc, best)).replace('aaaa', ('*' * len(inv_mapping.get(prob, ''))).ljust(4))

def get_all():
  return requests.get('https://robovinci.xyz/api/submissions', headers=headers).json()


def posts(prob, sol):
  files = {
      'file': sol,
  }

  response = requests.post('https://robovinci.xyz/api/submissions/%d/create'%(prob), headers=headers, files=files)
  return response

def get_sub(id):
  return requests.get('https://robovinci.xyz/api/submissions/%d'%(id), headers=headers).json()

def get_file(id):
  sub = get_sub(id)
  return requests.get(sub['file_url']).content

buckets = {}
recent = {}
def fill_buckets(rez=None):
  global buckets
  global recent
  buckets = {}
  recent = {}
  rez = rez or get_all()
  for i in rez['submissions']:
    prob = i['problem_id']
    if prob not in recent:
      recent[prob] = i
    if i['status'] != 'SUCCEEDED': continue
    if prob not in buckets:
      buckets[prob] = i
    if buckets[prob]['score'] > i['score']:
      buckets[prob] = i

def main():
  fill_buckets()
  rrrrr = range(1, len(buckets) + 1)
  for i in rrrrr:
    sub = buckets[i]
    print(sub)
    if False and recent[i]['id'] == sub['id']:
      print '%d Already best'%(i)
      continue

    file = get_file(sub['id'])
    merge = file.split('\n')
    last_color = -1
    if merge[0].startswith('color') and merge[1].startswith('color'):
      print (i, 'Double color')
      print(merge[:3])
      merge = merge[1:]
    elif i <= 25 and merge[0] == 'color [0] [255, 255, 255, 255]':
      print '%d Extra color'%(i)
      print merge[:3]
      merge = merge[1:]
    for ii in xrange(len(merge)):
      if merge[ii].startswith('color'):
        last_color = ii
    print (i, last_color, len(merge))
    merge = merge[:last_color+1]
    filee = '\n'.join(merge)
    if len(file) == len(filee):
      print (i, 'skipping')
      continue

    rez = posts(i, filee)
    print(rez.text)
    time.sleep(5)

if __name__ == '__main__':
  main()
