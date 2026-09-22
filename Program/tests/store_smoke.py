"""HTTP integration checks; run against a development API. Uses only stdlib."""
import json
import sys
import uuid
from urllib.error import HTTPError
from urllib.request import Request, urlopen

base = sys.argv[1] if len(sys.argv) > 1 else "http://localhost:5247"
checks = 0


def request(method, path, body=None, expected=200):
    global checks
    data = None if body is None else json.dumps(body).encode()
    req = Request(base + path, data=data, method=method,
                  headers={"Content-Type": "application/json"})
    try:
        response = urlopen(req, timeout=10)
    except HTTPError as error:
        response = error
    with response:
        raw = response.read()
        assert response.status == expected, (method, path, response.status, raw)
        checks += 1
        if expected == 201:
            assert response.headers.get("Location")
        result = json.loads(raw) if raw else None
        if expected >= 400:
            assert result["status"] == expected
        return result


paints = []
baskets = []
tag = uuid.uuid4().hex
payload = dict(name="White Wall", brand=tag, description="Washable interior coating",
               color="White", finish="Matte", surface="Wall", isExterior=False,
               volumeLiters=2.5, price=29.99, stockQuantity=10)
try:
    for changes in ({}, dict(name="Blue Wood", color="Blue", surface="Wood",
                            isExterior=True, price=45, stockQuantity=0)):
        paint = request("POST", "/api/paints", payload | changes, 201)
        paints.append(paint["id"])
    pid = paints[0]
    assert request("GET", f"/api/paints/{pid}")["description"] == payload["description"]
    query = f"/api/paints?brand={tag}"
    found = request("GET", query + "&search=WHITE%20washable&finish=matte&surface=wall&color=white&isExterior=false&inStock=true&minPrice=20&maxPrice=30&minVolumeLiters=2&maxVolumeLiters=3")
    assert found["totalCount"] == 1 and found["items"][0]["id"] == pid
    page = request("GET", query + "&sortBy=price&descending=true&pageSize=1")
    assert page["totalCount"] == 2 and page["items"][0]["id"] == paints[1]
    assert request("GET", query + "&sortBy=price&descending=true&pageSize=1&page=2")["items"][0]["id"] == pid
    assert request("GET", query + "&page=2147483647")["items"] == []
    assert request("GET", query + "&inStock=false")["items"][0]["id"] == paints[1]
    assert tag in request("GET", "/api/paints/lookups")["brands"]
    for invalid in ("page=0", "pageSize=101", "sortBy=unknown", "minPrice=5&maxPrice=1", "minVolumeLiters=3&maxVolumeLiters=1", "inStock=abc"):
        request("GET", "/api/paints?" + invalid, expected=400)
    for changes in (dict(name=" "), dict(price=-1), dict(volumeLiters=0), dict(stockQuantity=-1)):
        request("POST", "/api/paints", payload | changes, 400)
    basket = request("POST", "/api/baskets", expected=201)
    bid = basket["id"]
    baskets.append(bid)
    item = f"/api/baskets/{bid}/items/{pid}"
    for _ in range(2):
        view = request("PUT", item, {"quantity": 2})
        assert view["itemCount"] == 2 and view["total"] == 59.98
    request("PUT", item, {"quantity": 11}, 409)
    request("PUT", item, {"quantity": 0}, 400)
    request("PUT", f"/api/baskets/{bid}/items/{paints[1]}", {"quantity": 1}, 409)
    request("PUT", f"/api/paints/{pid}", payload | dict(stockQuantity=1), 409)
    request("PUT", f"/api/paints/{pid}", payload | dict(price=20))
    assert request("GET", f"/api/baskets/{bid}")["total"] == 40
    request("DELETE", item, expected=204)
    request("DELETE", item, expected=404)
    request("PUT", item, {"quantity": 1})
    request("DELETE", f"/api/baskets/{bid}/items", expected=204)
    assert request("GET", f"/api/baskets/{bid}")["total"] == 0
    request("PUT", item, {"quantity": 1})
    request("DELETE", f"/api/paints/{pid}", expected=204)
    paints.remove(pid)
    assert request("GET", f"/api/baskets/{bid}")["items"] == []
    request("GET", f"/api/paints/{pid}", expected=404)
    request("PUT", f"/api/paints/{pid}", payload, 404)
    request("PUT", item, {"quantity": 1}, 404)
    request("DELETE", f"/api/baskets/{bid}", expected=204)
    baskets.remove(bid)
    request("GET", f"/api/baskets/{bid}", expected=404)
    request("PUT", item, {"quantity": 1}, 404)
finally:
    for bid in baskets:
        request("DELETE", f"/api/baskets/{bid}", expected=204)
    for pid in paints:
        request("DELETE", f"/api/paints/{pid}", expected=204)

print(f"Passed {checks} HTTP checks, including CRUD, search, validation, and basket consistency.")
