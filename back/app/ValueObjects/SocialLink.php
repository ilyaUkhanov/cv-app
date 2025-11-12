<?php

namespace App\ValueObjects;

class SocialLink
{
    public function __construct(
        public string $kind,
        public string $url,
        public int $position = 0
    ) {}

    public function toArray(): array
    {
        return [
            'kind' => $this->kind,
            'url' => $this->url,
            'position' => $this->position,
        ];
    }

    public static function fromArray(array $data): self
    {
        return new self(
            kind: $data['kind'],
            url: $data['url'],
            position: $data['position'] ?? 0
        );
    }
}
