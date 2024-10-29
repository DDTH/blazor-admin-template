**The provided key/cert is for demo purposes only! Use your own key/cert for production.**

Generate a public-private keypair:

```bash
openssl req -x509 -newkey rsa:4096 -keyout keypair.pem -out cert.pem -days 3650
```

Extract the public key:

```bash
openssl x509 -pubkey -noout -in cert.pem > pubkey.pem
```

Create a .pfx file:

```bash
openssl pkcs12 -export -out cert.pfx -inkey keypair.pem -in cert.pem
```

The pass phrase for the demo private key and .pfx file is `Secret1`.
