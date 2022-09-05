import requests
import time

api_key = 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJlbWFpbCI6InRoZWNhdGlzbnVtYmVyMUBnb29nbGVncm91cHMuY29tIiwiZXhwIjoxNjYyMzczMjk3LCJvcmlnX2lhdCI6MTY2MjI4Njg5N30.dhJtIgf1DSzfJTK82DihkTL7expJRwgF6Akr0ixRR3Q'
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
def fill_buckets():
  global buckets
  global recent
  buckets = {}
  recent = {}
  rez = get_all()
  print rez
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
  for i in range(1, len(buckets) + 1):
    sub = buckets[i]
    if recent[i]['id'] == sub['id']:
      print '%d Already best'%(i)
      continue
    file = get_file(sub['id'])
    rez = posts(i, file)
    print(rez.text)
    time.sleep(5)

if __name__ == '__main__':
  get_report()